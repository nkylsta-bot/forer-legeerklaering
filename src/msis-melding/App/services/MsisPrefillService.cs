using System.Net.Http.Headers;
using Altinn.App.Core.Internal.Instances;
using Altinn.App.Models;
using Altinn.Platform.Storage.Interface.Models;
using Microsoft.Extensions.Caching.Memory;
using SmartFhir.Common;
using SmartFhir.Common.Session;

namespace Altinn.App.Services
{
    /// <summary>
    /// Forhåndsutfyller MSIS-meldingen fra EPJ-et med SMART-tokenet i legens sesjon: pasient,
    /// melder (lege og legekontor) og diagnosen i journalen. Hentingen fra FHIR ligger i
    /// SmartFhirPrefillClient (src/SmartFhir.Common).
    ///
    /// Felt legen ikke kan endre, overskrives fra journalen ved hver lesing. Felt legen kan endre,
    /// som kjønn og foreslått sykdom, fylles bare når de er tomme.
    /// </summary>
    public class MsisPrefillService : IDataProcessor
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMemoryCache _memoryCache;
        private readonly ILogger<MsisPrefillService> _logger;
        private readonly IInstanceClient _instanceClient;
        private readonly SmartFhirPrefillClient _fhirClient;

        public MsisPrefillService(
            IHttpClientFactory httpClientFactory,
            IHttpContextAccessor httpContextAccessor,
            IMemoryCache memoryCache,
            ILogger<MsisPrefillService> logger,
            IInstanceClient instanceClient
        )
        {
            _instanceClient = instanceClient;
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
            _memoryCache = memoryCache;
            _logger = logger;
            _fhirClient = new SmartFhirPrefillClient(logger);
        }

        public async Task ProcessDataRead(Instance instance, Guid? dataId, object data, string? language = null)
        {
            if (data is not MsisMeldingModel model)
                return;

            var session = _httpContextAccessor.HttpContext?.Session;
            var read = await SmartSessionReader.TryReadAsync(session, _memoryCache, _logger);
            if (read == null)
            {
                _logger.LogInformation("No SMART context found in session or cache — skipping FHIR pre-fill");
                return;
            }

            if (!await BindLaunchContextToInstance(instance, read.Context))
                return;

            _logger.LogInformation(
                "Starting FHIR pre-fill: patient={Patient}, encounter={Encounter}",
                read.Context?.PatientId,
                read.Context?.EncounterId
            );

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                "Bearer",
                read.Token.AccessToken
            );

            var fhirData = await _fhirClient.FetchAsync(client, read.Context);

            // Fra journalen, ikke redigerbare
            model.Pasient_Fnr = fhirData.Pasient_Fnr;
            model.Pasient_Dnr = fhirData.Pasient_Dnr;
            model.Pasient_Fornavn = fhirData.Pasient_Fornavn;
            model.Pasient_Etternavn = fhirData.Pasient_Etternavn;
            model.Pasient_Fodselsdato = fhirData.Pasient_Fodselsdato;
            model.Melder_Navn = string.Join(
                " ",
                new[] { fhirData.Henviser_Fornavn, fhirData.Henviser_Etternavn }.Where(n => !string.IsNullOrEmpty(n))
            );
            model.Melder_Hpr = fhirData.Henviser_Hpr;
            model.Melder_Virksomhet = fhirData.Virksomhet_Navn;
            model.Melder_Orgnr = fhirData.Virksomhet_Orgnr;
            model.Epj_KonsultasjonDato = fhirData.Konsultasjon_Dato;
            model.Epj_DiagnoseKode = fhirData.Diagnose_Icd10Kode;
            model.Epj_DiagnoseTekst = fhirData.Diagnose_Icd10Tekst;

            // Forslag legen kan endre
            model.Pasient_Kjonn ??= fhirData.Pasient_Kjonn switch
            {
                "male" => "mann",
                "female" => "kvinne",
                null => null,
                _ => "ukjent",
            };
            model.Sykdom ??= MsisSykdommer.FraIcd10(fhirData.Diagnose_Icd10Kode)?.Kode;
        }

        /// <summary>
        /// Lagrer launch-konteksten (FHIR-adresse, pasient og konsultasjon) i instansens dataValues
        /// første gang skjemaet leses med en SMART-sesjon. Skrivingen tilbake til EPJ-et
        /// (EpjWritebackServiceTask) leser dem derfra, fordi den kjører uten legens sesjon.
        /// Returnerer false hvis instansen allerede tilhører en annen pasient eller konsultasjon enn
        /// sesjonen. Da hoppes forhåndsutfyllingen over, så et eldre skjema ikke fylles med en annen
        /// pasient fra en senere launch.
        /// </summary>
        private async Task<bool> BindLaunchContextToInstance(Instance instance, FhirLaunchContext context)
        {
            var values = instance.DataValues ?? new Dictionary<string, string>();
            values.TryGetValue(SmartInstanceDataValues.PatientId, out var boundPatient);
            values.TryGetValue(SmartInstanceDataValues.EncounterId, out var boundEncounter);

            if (!string.IsNullOrEmpty(boundPatient))
            {
                if (boundPatient == context.PatientId && boundEncounter == context.EncounterId)
                    return true;
                _logger.LogWarning(
                    "Instance {InstanceId} belongs to another patient or encounter than the SMART session — skipping pre-fill",
                    instance.Id
                );
                return false;
            }

            var parts = instance.Id.Split('/');
            await _instanceClient.UpdateDataValues(
                int.Parse(parts[0], System.Globalization.CultureInfo.InvariantCulture),
                Guid.Parse(parts[1]),
                new DataValues
                {
                    Values = new Dictionary<string, string?>
                    {
                        [SmartInstanceDataValues.FhirBaseUrl] = context.FhirBaseUrl,
                        [SmartInstanceDataValues.PatientId] = context.PatientId,
                        [SmartInstanceDataValues.EncounterId] = context.EncounterId,
                    },
                }
            );
            _logger.LogInformation("Bound SMART launch context to instance {InstanceId}", instance.Id);
            return true;
        }

        // Forhåndsutfyllingen skjer bare ved lesing.
        public Task ProcessDataWrite(
            Instance instance,
            Guid? dataId,
            object data,
            object? previousData,
            string? language = null
        )
        {
            return Task.CompletedTask;
        }
    }
}
