using System.Net.Http.Headers;
using Altinn.App.Models;
using Altinn.Platform.Storage.Interface.Models;
using Microsoft.Extensions.Caching.Memory;
using SmartFhir.Common;

namespace Altinn.App.Services
{
    /// <summary>
    /// Pre-fills ForerLegeerklaeringModel with data from FHIR resources fetched from the EPJ
    /// using the SMART access token stored in server session.
    ///
    /// Selve FHIR-hentingen/parsingen (Patient/Practitioner/Organization/Encounter) ligger nå i
    /// SmartFhirPrefillClient (src/SmartFhir.Common) — denne klassen mapper kun det resultatet
    /// over på ForerLegeerklaeringModel sine feltnavn, og beholder det som er spesifikt for
    /// nettopp dette caset: Diagnose (Condition) og hele ForerKonklusjonModel-avledningen ved
    /// signering (se ForerKonklusjonTaskEndingHandler), som kjeveortopedisk-caset ikke har noe tilsvarende av.
    /// </summary>
    public class FhirPrefillService : IDataProcessor
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMemoryCache _memoryCache;
        private readonly ILogger<FhirPrefillService> _logger;
        private readonly SmartFhirPrefillClient _fhirClient;

        public FhirPrefillService(
            IHttpClientFactory httpClientFactory,
            IHttpContextAccessor httpContextAccessor,
            IMemoryCache memoryCache,
            ILogger<FhirPrefillService> logger
        )
        {
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
            _memoryCache = memoryCache;
            _logger = logger;
            _fhirClient = new SmartFhirPrefillClient(logger);
        }

        public async Task ProcessDataRead(Instance instance, Guid? dataId, object data, string? language = null)
        {
            _logger.LogInformation("FhirPrefillService.ProcessDataRead called for instance {InstanceId}", instance?.Id);

            if (data is not ForerLegeerklaeringModel model)
            {
                _logger.LogInformation("Data is not ForerLegeerklaeringModel — skipping");
                return;
            }

            var session = _httpContextAccessor.HttpContext?.Session;
            var read = await SmartSessionReader.TryReadAsync(session, _memoryCache, _logger);
            if (read == null)
            {
                _logger.LogInformation("No SMART context found in session or cache — skipping FHIR pre-fill");
                return;
            }

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

            model.Pasient_Fnr = fhirData.Pasient_Fnr;
            model.Pasient_Fornavn = fhirData.Pasient_Fornavn;
            model.Pasient_Etternavn = fhirData.Pasient_Etternavn;
            model.Pasient_Fodselsdato = fhirData.Pasient_Fodselsdato;
            model.Pasient_Kjonn = fhirData.Pasient_Kjonn;

            model.Lege_HPR = fhirData.Henviser_Hpr;
            model.Lege_Fornavn = fhirData.Henviser_Fornavn;
            model.Lege_Etternavn = fhirData.Henviser_Etternavn;

            model.Virksomhet_Navn = fhirData.Virksomhet_Navn;
            model.Virksomhet_Orgnr = fhirData.Virksomhet_Orgnr;
            model.Virksomhet_HerId = fhirData.Virksomhet_HerId;

            model.Konsultasjon_Dato = fhirData.Konsultasjon_Dato;

            model.Diagnose_Kode = fhirData.Diagnose_Kode;
            model.Diagnose_Tekst = fhirData.Diagnose_Tekst;
        }

        // BUG (2026-08-12): avledningen av ForerKonklusjonModel lå tidligere her, i
        // IDataProcessor.ProcessDataWrite. Den kjører på HVER autolagring mens legen fyller ut
        // skjemaet — ikke bare når legen faktisk trykker "Signer og send inn". SVV-konklusjonen
        // ble dermed skrevet/oppdatert gjentatte ganger under utfylling, ikke bare ved innsending.
        // Flyttet til ForerKonklusjonTaskEndingHandler (IOnTaskEndingHandler), som kjører når Task_1
        // (signeringsoppgaven) faktisk avsluttes. ProcessDataWrite er nå en no-op, men må fortsatt implementeres siden
        // IDataProcessor krever den.
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
