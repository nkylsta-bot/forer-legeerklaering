using SmartFhir.Common;

namespace Altinn.App.Controllers
{
    /// <summary>
    /// SMART on FHIR launch/callback for MSIS-meldingen. All OAuth2/PKCE/session-mekanikk ligger i
    /// SmartLaunchControllerBase (src/SmartFhir.Common). Denne klassen setter bare det som er
    /// spesifikt for meldingen: klient-ID og scopes. Condition trengs for å foreslå sykdommen fra
    /// diagnosen, og DocumentReference for å lagre en kopi av meldingen i journalen, slik skjemaet krever.
    /// </summary>
    public class SmartLaunchController : SmartLaunchControllerBase
    {
        public SmartLaunchController(
            IHttpClientFactory httpClientFactory,
            IConfiguration config,
            Microsoft.Extensions.Caching.Memory.IMemoryCache memoryCache,
            ILogger<SmartLaunchController> logger,
            IWebHostEnvironment env
        )
            : base(httpClientFactory, config, memoryCache, logger, env) { }

        protected override string DefaultClientId => "msis-melding-poc";

        // Testdata for /smart/test-prefill og /smart/dev-login mot repoets egen FHIR-mock
        protected override string DefaultTestPatientId => "sophie-salt";
        protected override string DefaultTestEncounterId => "enc-sophie-001";
        protected override string DefaultTestPractitionerPath => "Practitioner/lege-ola";

        protected override IEnumerable<string> GetScopes()
        {
            foreach (var scope in base.GetScopes())
                yield return scope;

            yield return "patient/Condition.read";
            yield return "patient/DocumentReference.write";
        }
    }
}
