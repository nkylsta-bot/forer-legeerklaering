using Altinn.App.Core.Features.Process;
using Altinn.App.Models;

namespace Altinn.App.Services
{
    /// <summary>
    /// Avleder og lagrer ForerKonklusjonModel når Task_1 (utfylling og signering) avsluttes —
    /// altså først når legen faktisk sender inn, ikke ved hver autolagring under utfylling.
    ///
    /// Erstatter den tidligere IProcessTaskEnd-implementasjonen i FhirPrefillService (fjernet i v9).
    /// Idempotent: finnes ForerKonklusjon-elementet fra før, overskrives innholdet med en ny avledning
    /// fra skjemadataene; ellers opprettes det. Workflow-motoren kan derfor trygt kjøre handleren på nytt.
    /// </summary>
    public class ForerKonklusjonTaskEndingHandler : IOnTaskEndingHandler
    {
        private const string TaskId = "Task_1";
        private const string LegeerklaeringDataType = "ForerLegeerklaering";
        private const string KonklusjonDataType = "ForerKonklusjon";

        private readonly ILogger<ForerKonklusjonTaskEndingHandler> _logger;

        // Kjøretøygrupper som tilhører gruppe 1 (lette kjøretøy)
        private static readonly HashSet<string> Gruppe1Koder = new(StringComparer.OrdinalIgnoreCase)
        {
            "A",
            "A1",
            "A2",
            "AM",
            "B",
            "B1",
            "BE",
            "S",
            "T",
        };

        // Kjøretøygrupper som tilhører gruppe 2 (tunge kjøretøy)
        private static readonly HashSet<string> Gruppe2Koder = new(StringComparer.OrdinalIgnoreCase)
        {
            "C",
            "C1",
            "CE",
            "C1E",
        };

        // Kjøretøygrupper som tilhører gruppe 3 (persontransport/utrykking)
        private static readonly HashSet<string> Gruppe3Koder = new(StringComparer.OrdinalIgnoreCase)
        {
            "D",
            "D1",
            "DE",
            "D1E",
        };

        public ForerKonklusjonTaskEndingHandler(ILogger<ForerKonklusjonTaskEndingHandler> logger)
        {
            _logger = logger;
        }

        public bool ShouldRunForTask(string taskId) =>
            string.Equals(taskId, TaskId, StringComparison.OrdinalIgnoreCase);

        public async Task<HandlerResult> Execute(OnTaskEndingContext context)
        {
            var mutator = context.InstanceDataMutator;
            var instance = mutator.Instance;

            var legeerklaeringElement = instance.Data?.Find(d =>
                d.DataType.Equals(LegeerklaeringDataType, StringComparison.OrdinalIgnoreCase)
            );
            if (legeerklaeringElement == null)
            {
                return HandlerResult.FailedPermanent(
                    $"Fant ikke {LegeerklaeringDataType}-dataelement på instans {instance.Id}"
                );
            }

            if (await mutator.GetFormData(legeerklaeringElement) is not ForerLegeerklaeringModel src)
            {
                return HandlerResult.FailedPermanent(
                    $"{LegeerklaeringDataType}-dataelementet på instans {instance.Id} kunne ikke deserialiseres"
                );
            }

            var derived = DeriveKonklusjon(src);

            var konklusjonElement = instance.Data?.Find(d =>
                d.DataType.Equals(KonklusjonDataType, StringComparison.OrdinalIgnoreCase)
            );

            if (konklusjonElement != null)
            {
                // Modellen fra mutatoren er sporet: endringer lagres automatisk når handleren lykkes.
                if (await mutator.GetFormData(konklusjonElement) is not ForerKonklusjonModel existing)
                {
                    return HandlerResult.FailedPermanent(
                        $"{KonklusjonDataType}-dataelementet {konklusjonElement.Id} kunne ikke deserialiseres"
                    );
                }

                _logger.LogInformation("Oppdaterer eksisterende ForerKonklusjon {Id}", konklusjonElement.Id);
                CopyInto(derived, existing);
            }
            else
            {
                _logger.LogInformation("Oppretter ForerKonklusjon på instans {InstanceId}", instance.Id);
                mutator.AddFormDataElement(KonklusjonDataType, derived);
            }

            return HandlerResult.Success();
        }

        private static ForerKonklusjonModel DeriveKonklusjon(ForerLegeerklaeringModel src)
        {
            var resultat = src.Forer_ErSkikket == true ? "skikket" : "ikke_skikket";
            var gruppe = src.Forer_Kjoretoygruppe ?? "";

            return new ForerKonklusjonModel
            {
                Pasient_Fnr = src.Pasient_Fnr,
                Lege_HPR = src.Lege_HPR,
                Gruppe1_Resultat = Gruppe1Koder.Contains(gruppe) ? resultat : "",
                Gruppe2_Resultat = Gruppe2Koder.Contains(gruppe) ? resultat : "",
                Gruppe3_Resultat = Gruppe3Koder.Contains(gruppe) ? resultat : "",
                Vilkar = src.Forer_Vilkar,
                Merknad = src.Forer_Merknad,
            };
        }

        private static void CopyInto(ForerKonklusjonModel from, ForerKonklusjonModel to)
        {
            to.Pasient_Fnr = from.Pasient_Fnr;
            to.Lege_HPR = from.Lege_HPR;
            to.Gruppe1_Resultat = from.Gruppe1_Resultat;
            to.Gruppe1_BegrensetAntallAar = from.Gruppe1_BegrensetAntallAar;
            to.Gruppe2_Resultat = from.Gruppe2_Resultat;
            to.Gruppe2_BegrensetAntallAar = from.Gruppe2_BegrensetAntallAar;
            to.Gruppe3_Resultat = from.Gruppe3_Resultat;
            to.Gruppe3_BegrensetAntallAar = from.Gruppe3_BegrensetAntallAar;
            to.Vilkar = from.Vilkar;
            to.Merknad = from.Merknad;
        }
    }
}
