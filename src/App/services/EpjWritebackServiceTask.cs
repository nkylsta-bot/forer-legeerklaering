using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Altinn.App.Core.Features.Process;
using Altinn.App.Core.Models;
using Altinn.App.Models;
using Altinn.Platform.Storage.Interface.Models;
using SmartFhir.Common.Backend;
using SmartFhir.Common.Session;

namespace Altinn.App.Services
{
    /// <summary>
    /// Skriver den genererte legeerklæringen (PDF) tilbake til EPJ-et som et journalnotat på
    /// konsultasjonen appen ble startet fra. Kjører som en egen service task etter PDF-oppgaven,
    /// slik at PDF-en finnes.
    ///
    /// Service tasks kjører i workflow-motorens callback, ikke i legens forespørsel, så legens
    /// SMART-sesjon er ikke tilgjengelig. Oppgaven henter derfor et eget systemtoken med SMART
    /// Backend Services, og leser pasient og konsultasjon fra instansens dataValues, som
    /// FhirPrefillService satte ved launch.
    ///
    /// Instanser som ikke ble startet fra et EPJ, har ingen launch-kontekst, og hoppes over.
    /// </summary>
    public sealed class EpjWritebackServiceTask : IServiceTask
    {
        public const string TaskType = "epjWriteback";

        private const string PdfDataType = "ref-data-as-pdf";
        private const string LegeerklaeringDataType = "ForerLegeerklaering";

        // Journalnotat i Volven-kodeverk 9602, den eneste dokumenttypen nav-epj tar imot.
        private const string DocumentTypeSystem = "urn:oid:2.16.578.1.12.4.1.1.9602";
        private const string DocumentTypeCode = "J01-2";

        private readonly SmartBackendServicesClient _backendClient;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<EpjWritebackServiceTask> _logger;

        public EpjWritebackServiceTask(
            SmartBackendServicesClient backendClient,
            IHttpClientFactory httpClientFactory,
            ILogger<EpjWritebackServiceTask> logger
        )
        {
            _backendClient = backendClient;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public string Type => TaskType;

        public ServiceTaskPipeline DefineExecute(PipelineBuilder pipeline) => pipeline.Finally(Execute);

        private async Task<ServiceTaskResult> Execute(StageContext context)
        {
            var mutator = context.InstanceDataMutator;
            var instance = mutator.Instance;
            var values = instance.DataValues ?? new Dictionary<string, string>();
            values.TryGetValue(SmartInstanceDataValues.FhirBaseUrl, out var fhirBaseUrl);
            values.TryGetValue(SmartInstanceDataValues.PatientId, out var patientId);
            values.TryGetValue(SmartInstanceDataValues.EncounterId, out var encounterId);

            if (
                string.IsNullOrEmpty(fhirBaseUrl)
                || string.IsNullOrEmpty(patientId)
                || string.IsNullOrEmpty(encounterId)
            )
            {
                _logger.LogInformation(
                    "Instance {InstanceId} has no SMART launch context — skipping EPJ write-back",
                    instance.Id
                );
                return ServiceTaskResult.Success();
            }

            var pdfElement = instance.Data?.FindLast(d => d.DataType == PdfDataType);
            if (pdfElement is null)
                return ServiceTaskResult.FailedPermanent("No generated PDF to write back to the EPJ");

            var pdf = await mutator.GetBinaryData(pdfElement);
            var form = await ReadForm(mutator, instance);

            try
            {
                var token = await _backendClient.GetSystemAccessTokenAsync(
                    fhirBaseUrl,
                    ["system/DocumentReference.c"],
                    context.CancellationToken
                );
                var location = await PostDocumentReference(
                    fhirBaseUrl,
                    token,
                    BuildDocumentReference(patientId, encounterId, Description(instance, form), pdfElement, pdf),
                    context.CancellationToken
                );
                _logger.LogInformation(
                    "Wrote the legeerklæring for instance {InstanceId} back to the EPJ: {Location}",
                    instance.Id,
                    location
                );
                return ServiceTaskResult.Success();
            }
            catch (SmartBackendException e) when (e.IsTransient)
            {
                _logger.LogWarning(e, "EPJ write-back failed for instance {InstanceId}, will retry", instance.Id);
                return ServiceTaskResult.FailedRetryable(e.Message);
            }
            catch (SmartBackendException e)
            {
                _logger.LogError(e, "EPJ write-back rejected for instance {InstanceId}", instance.Id);
                return ServiceTaskResult.FailedPermanent(e.Message);
            }
            catch (HttpRequestException e)
            {
                _logger.LogWarning(e, "EPJ unreachable for instance {InstanceId}, will retry", instance.Id);
                return ServiceTaskResult.FailedRetryable(e.Message);
            }
        }

        private static async Task<ForerLegeerklaeringModel?> ReadForm(IInstanceDataMutator mutator, Instance instance)
        {
            var element = instance.Data?.Find(d => d.DataType == LegeerklaeringDataType);
            return element is null ? null : await mutator.GetFormData(element) as ForerLegeerklaeringModel;
        }

        /// <summary>Notatteksten som vises i journalen. Selve legeerklæringen ligger i PDF-vedlegget.</summary>
        private static string Description(Instance instance, ForerLegeerklaeringModel? form)
        {
            var reference = instance.Id.Split('/').Last().Split('-').Last();
            var text = new StringBuilder(
                $"Legeerklæring førerrett (IS-2569) sendt inn i Altinn, referanse {reference}."
            );
            if (form?.Forer_ErSkikket is bool skikket)
            {
                var gruppe = string.IsNullOrEmpty(form.Forer_Kjoretoygruppe)
                    ? ""
                    : $" for førerkortklasse {form.Forer_Kjoretoygruppe}";
                text.Append(
                    skikket
                        ? $" Pasienten er vurdert som helsemessig skikket{gruppe}."
                        : $" Pasienten er vurdert som ikke helsemessig skikket{gruppe}."
                );
            }
            if (!string.IsNullOrWhiteSpace(form?.Forer_Vilkar))
                text.Append($" Vilkår: {form.Forer_Vilkar.Trim()}");
            return text.ToString();
        }

        private static Dictionary<string, object?> BuildDocumentReference(
            string patientId,
            string encounterId,
            string description,
            DataElement pdfElement,
            ReadOnlyMemory<byte> pdf
        ) =>
            new()
            {
                ["resourceType"] = "DocumentReference",
                ["status"] = "current",
                ["type"] = new Dictionary<string, object?>
                {
                    ["coding"] = new object[]
                    {
                        new Dictionary<string, object?>
                        {
                            ["system"] = DocumentTypeSystem,
                            ["code"] = DocumentTypeCode,
                        },
                    },
                },
                ["subject"] = new Dictionary<string, object?> { ["reference"] = $"Patient/{patientId}" },
                ["description"] = description,
                ["content"] = new object[]
                {
                    new Dictionary<string, object?>
                    {
                        ["attachment"] = new Dictionary<string, object?>
                        {
                            ["contentType"] = "application/pdf",
                            ["title"] = pdfElement.Filename ?? "Legeerklæring førerrett.pdf",
                            ["data"] = Convert.ToBase64String(pdf.Span),
                        },
                    },
                },
                ["context"] = new Dictionary<string, object?>
                {
                    ["encounter"] = new object[]
                    {
                        new Dictionary<string, object?> { ["reference"] = $"Encounter/{encounterId}" },
                    },
                },
            };

        private async Task<string?> PostDocumentReference(
            string fhirBaseUrl,
            string token,
            Dictionary<string, object?> documentReference,
            CancellationToken cancellationToken
        )
        {
            var client = _httpClientFactory.CreateClient();
            using var request = new HttpRequestMessage(HttpMethod.Post, $"{fhirBaseUrl.TrimEnd('/')}/DocumentReference")
            {
                Content = new StringContent(
                    JsonSerializer.Serialize(documentReference),
                    Encoding.UTF8,
                    "application/fhir+json"
                ),
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            using var response = await client.SendAsync(request, cancellationToken);
            if (response.StatusCode != HttpStatusCode.Created)
            {
                var body = await response.Content.ReadAsStringAsync(cancellationToken);
                throw new SmartBackendException(
                    $"POST DocumentReference failed: {(int)response.StatusCode} {body}",
                    response.StatusCode
                );
            }
            return response.Headers.Location?.ToString();
        }
    }
}
