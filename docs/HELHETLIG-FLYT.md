# Helhetlig flyt: Pasient → Helsenorge → EPJ/Altinn → SVV

**Dato:** 2026-08-11, oppdatert 2026-11-08 med nav-epj og veien mot TT02
**Formål:** Binde sammen de fire aktørene som til nå har vært beskrevet i separate dokumenter — Altinn Studio-appen, Helsenorge.no, EPJ-systemet og Statens vegvesen (SVV) — i én sammenhengende beskrivelse. Ingen enkelt eksisterende dokument viser hele reisen; se «Kilder for hvert steg» nederst for hvor detaljene faktisk står.

---

## De fire aktørene

| Aktør | Rolle i flyten | Eies/driftes av |
|---|---|---|
| **Pasient (Helsenorge.no)** | Fyller ut egenerklæring om helse (NA-0201) før konsultasjon | Innbygger, via Helsenorge-plattformen (NHN) |
| **EPJ (fastlegesystem)** | Legens journalsystem — starter SMART-launch, tilbyr FHIR-data | Fastlegekontoret, EPJ-leverandør (CGM/Infodoc/WebMed) |
| **Altinn Studio-app** (`forer-legeerklaering`) | BFF som henter FHIR-data, prefiller IS-2569, styrer signering og innsending | Digdir (denne PoC-en) |
| **Statens vegvesen (SVV) / Helsedirektoratet** | Mottar konklusjonen om skikkethet for førerkort | Statens vegvesen (førerkortregisteret) |

---

## Helhetlig sekvens

```
Pasient (Helsenorge.no)      EPJ (fastlege)         Altinn Studio-app (BFF)      SVV / Helsedirektoratet
        │                          │                          │                          │
        │ 1. Fyller ut             │                          │                          │
        │ egenerklæring            │                          │                          │
        │ (Oppgave+Skjema,         │                          │                          │
        │ eller Dialogporten)      │                          │                          │
        │─────────────────────────►│ (synkes til EPJ,         │                          │
        │  [PLANLAGT — ikke        │  evt. hentes direkte     │                          │
        │  implementert i PoC]     │  av Altinn-appen)        │                          │
        │                          │                          │                          │
        │                          │ 2. Lege starter          │                          │
        │                          │ konsultasjon,             │                          │
        │                          │ SMART EHR Launch          │                          │
        │                          │─────────────────────────►│                          │
        │                          │  [VERIFISERT — SmartLaunchController]                │
        │                          │                          │                          │
        │                          │◄─────────────────────────│ 3. Henter FHIR-data       │
        │                          │  Patient, Practitioner,   │  (Patient/Practitioner/   │
        │                          │  Organization, Encounter,  │  Organization/Encounter/  │
        │                          │  Condition, evt.           │  Condition)               │
        │                          │  QuestionnaireResponse     │  [VERIFISERT — FhirPrefillService]
        │                          │  (egenerklæring)           │  [Egenerklæring-henting: PLANLAGT]
        │                          │                          │                          │
        │                          │                          │ 4. Prefiller IS-2569,     │
        │                          │                          │ legen kontrollerer,       │
        │                          │                          │ supplerer, signerer       │
        │                          │                          │ («Signer og send inn»)    │
        │                          │                          │  [VERIFISERT — process.bpmn Task_1]
        │                          │                          │                          │
        │                          │◄─────────────────────────│ 5a. Full attest skrives   │
        │                          │  DocumentReference         │  tilbake til EPJ          │
        │                          │  [PLANLAGT — VEIKART.md   │                          │
        │                          │  fase 2, ikke kodet]      │                          │
        │                          │                          │                          │
        │                          │                          │ 5b. Konklusjon (grønt/    │
        │                          │                          │ rødt per kjøretøygruppe) │
        │                          │                          │──────────────────────────►│
        │                          │                          │  Altinn Events →          │
        │                          │                          │  SVV henter selv via      │
        │                          │                          │  Maskinporten             │
        │                          │                          │  [MODELL VERIFISERT —     │
        │                          │                          │  ForerKonklusjonModel;    │
        │                          │                          │  SVV-abonnement PLANLAGT] │
```

**Steg 1** kan alternativt gjøres via to forskjellige spor — se [PASIENTFLYT.md](PASIENTFLYT.md) for begge:
- **Alternativ A:** Dialogporten-dialog vist på helsenorge.no, egenerklæring fylt ut i en egen Altinn-app.
- **Alternativ B:** Helsenorge EksternAPI (Oppgave + Skjema) — teknisk autentisering og selve API-strukturen er verifisert, og en informasjonsoppgave (`focus.type = "Communication"`) er nå bekreftet levert ende-til-ende til en ekte testpersons Helsenorge-innboks (se [IMPLEMENTERING.md §14.1](IMPLEMENTERING.md)). Selve skjemaoppgaven (`Questionnaire`/`Bundle`), som er det NA-0201 faktisk krever, er ikke bygget ennå — men den tidligere NHN-kontakt-blokkeringen er løst (se [BESLUTNINGER.md C-6](BESLUTNINGER.md)).

---

## Status: hva er faktisk verifisert vs. planlagt

| # | Steg | Status | Kilde |
|---|---|---|---|
| 1 | Pasient fyller ut egenerklæring via Helsenorge | ❌ Ikke implementert — kun arkitekturforslag (to alternativer) | [PASIENTFLYT.md](PASIENTFLYT.md) |
| 2 | EPJ → Altinn: SMART EHR Launch, full ende-til-ende | ✅ **Fullstendig verifisert 2026-08-11** mot [launch.smarthealthit.org](https://launch.smarthealthit.org/) — innlogging, pasientvalg, callback, Altinn-sesjon og full FHIR-prefill (pasient/lege/virksomhet). 7 bugs funnet og rettet | [IMPLEMENTERING.md §13](IMPLEMENTERING.md), [SmartLaunchController.cs](../src/App/controllers/SmartLaunchController.cs) |
| 2c | EPJ → Altinn mot NAVs `nav-epj` med norske testdata | ✅ **Verifisert lokalt 2026-11-08.** Launch fra nav-epj, token-utveksling, validert id_token og full prefill av pasient, lege, legekontor, konsultasjon og diagnose. Krevde tre rettelser i appen og én lokal linjeendring i nav-epj, se under | [LOKALT-MED-NAV-EPJ.md](LOKALT-MED-NAV-EPJ.md) |
| 2b | Altinn-sesjon etableres for legen | ⚠️ Mitigert i dev (auto-innlogging via localtest-testbruker i `/smart/callback`), men ikke en produksjonsløsning — se §14 for det egentlige forslaget (HelseID-identitet) | [RISIKOREGISTER.md R10](RISIKOREGISTER.md), [VEIKART.md fase 1](VEIKART.md) |
| 3 | Altinn henter FHIR-data fra EPJ | ✅ Verifisert mot lokal HAPI FHIR-mock, og 2026-11-08 mot nav-epj med norske OID-er for fødselsnummer, HPR-nummer og organisasjonsnummer. Ikke mot et reelt fastlege-EPJ | [FhirPrefillService.cs](../src/App/services/FhirPrefillService.cs), [RISIKOREGISTER.md R1](RISIKOREGISTER.md) |
| 3b | Altinn henter pasientens egenerklæring (QuestionnaireResponse) | ❌ Ikke implementert — avhenger av steg 1 | [PASIENTFLYT.md §3](PASIENTFLYT.md) |
| 4 | Lege fyller ut, signerer, sender inn | ✅ Verifisert («Signer og send inn», Task_1) | [process.bpmn](../src/App/config/process/process.bpmn) |
| 5a | Full attest skrives tilbake til EPJ | ⚠️ Skrivemekanikk bevist 2026-08-11 (`POST DocumentReference` → `HTTP 201` mot launch.smarthealthit.org), men placeholder-innhold, ikke PDF/idempotens. Mot nav-epj gir testen `HTTP 400`, fordi nav-epj bare godtar sitt eget notatformat | [VEIKART.md fase 2](VEIKART.md), [IMPLEMENTERING.md §13](IMPLEMENTERING.md) |
| 5b | Konklusjon (grønt/rødt) → SVV via Altinn Events | ⚠️ Datamodell verifisert (`ForerKonklusjonModel`), selve Events-abonnementet hos SVV er ikke avtalt | [BESLUTNINGER.md C-3](BESLUTNINGER.md) |
| — | Helsenorge EksternAPI (Oppgave/Skjema) | ✅ **Ende-til-ende verifisert 2026-09-18/21** for `Communication`- og `Questionnaire`-oppgave, samt kopi-tilbake til innbygger (`DocumentReference`, eget API). Gjenstår: `Bundle`-varianten, HTTPS-nåbart skjema (localtest er ren HTTP), og sømløst OIDC-uthopp | [IMPLEMENTERING.md §14.1](IMPLEMENTERING.md), [RISIKOREGISTER.md R9](RISIKOREGISTER.md), [PASIENTFLYT.md](PASIENTFLYT.md) |

**Lesbar oppsummering:** Den *midtre* delen av flyten (steg 2–4, lege ↔ Altinn-app) er den best verifiserte delen av hele PoC-en. Det som skjer *før* (pasientens egenerklæring) og *etter* (writeback til EPJ, faktisk mottak hos SVV) konsultasjonen er i stor grad arkitektur og datamodeller — ikke virkende integrasjoner.

---

## nav-epj og veien mot TT02 (2026-11-08)

Appen er koblet til NAVs test-EPJ [`navikt/nav-epj`](https://github.com/navikt/nav-epj), som bruker norske identifikatorer og testdata. Oppsett og testscenarioer står i [LOKALT-MED-NAV-EPJ.md](LOKALT-MED-NAV-EPJ.md). Analysen fra august, [NAV-EPJ-TESTMILJO.md](NAV-EPJ-TESTMILJO.md), er utdatert. [smart-launch-nav-epj.svg](smart-launch-nav-epj.svg) viser flyten steg for steg.

### Hva som er endret i nav-epj siden august

- Feilene og patchen fra august gjelder ikke lenger. Patchen lar seg ikke anvende, og koden er rettet oppstrøms.
- En launch må starte fra nav-epj sitt eget grensesnitt eller API, med en valgt pasient og en aktiv konsultasjon. Launchen er bundet til behandleren og legekontoret.
- NAV har en åpen, driftet instans på `https://epj.ekstern.dev.nav.no`. SMART- og FHIR-endepunktene krever ikke NAV-innlogging, og brukerne logger inn med HelseID-testbehandlere. NAV registrerer hver klient manuelt, og det krever en utrulling hos dem.

### Endringer i appen

Endringene ligger i `SmartFhir.Common` og gjelder begge appene.

| Endring | Hvorfor |
|---|---|
| fhirUser leses fra id_token, som valideres mot EPJ-ens `jwks_uri`, utsteder, `client_id` og levetid | nav-epj gir fhirUser bare i id_token. Uten dette ble lege og legekontor ikke fylt ut |
| `iss` brukes som FHIR-adresse. `FhirBaseUrlOverride` gjelder bare for utstedere i den nye `FhirBaseUrlOverrideIssuers` | Overstyringen ble brukt for alle http-utstedere, også en lokal nav-epj, og prefill gikk til vår egen HAPI-mock |
| `state` lages som URL-sikker base64 | nav-epj URL-koder ikke `state` i callback-redirecten. En `+` ble til mellomrom og ga «State mismatch» omtrent annenhver gang |

### Verifisert lokalt

| Scenario | Resultat |
|---|---|
| Forhåndsutfylling for GLAD FJORD | ✅ Pasient, lege (GRØNN VITS, HPR 565501872), legekontor, konsultasjon og diagnose fylt ut |
| Bytte til TRYGG FJORD | ✅ Nytt skjema med riktig pasient |
| Gjenbrukt launch-lenke | ✅ Avvist med «Authorization error: invalid_request» |
| Feil klienthemmelighet | ✅ «Token exchange failed» |
| Innsending | ✅ PDF og kvittering 2026-11-09. Krevde at PDF-containeren på testmaskinen stolte på maskinens TLS-proxy, ingen endring i appen |

### Funn

- nav-epj godtar http-adresser for klienter bare på `localhost` og `127.0.0.1`. Localtest finner appen bare på `local.altinn.cloud`, så nav-epj må endres lokalt på én linje. Et forslag til NAV er å godta adresser som peker til loopback.
- nav-epj sitt `state`-problem bør meldes til NAV, selv om appen nå tåler det.
- `/smart/test-writeback` er skrevet for Epic og gir `HTTP 400` mot nav-epj.
- Skjemaet merker diagnosekoden som ICD-10, men nav-epj sender ICPC-2-koder, for eksempel T86.
- nav-epj sin frontend henter NAVs designsystem fra GitHub Packages og krever et GitHub-token med `read:packages`.

### Hva som gjenstår før TT02

| # | Tiltak | Hvorfor |
|---|---|---|
| 1 | Eget Altinn Studio-repo under `digdir`, med appen i `App/` på roten og en Dockerfile. `SmartFhir.Common` kopieres inn eller publiseres som NuGet-pakke | TT02-utrulling går gjennom Altinn Studio, som bygger repoet fra roten. Dette repoet har appen under `src/App` og en prosjektreferanse utenfor den |
| 2 | Akseptere dobbel innlogging i første omgang: HelseID hos nav-epj, deretter ID-porten i Altinn med en TT02-testbruker | Callbacken lager Altinn-sesjonen med en snarvei som bare finnes i localtest (R10). Én innlogging er fase 1 i [VEIKART.md](VEIKART.md) |
| 3 | Flytte SMART-tilstand og token fra serverminnet til en kryptert informasjonskapsel, eller kjøre én replika i demoen | TT02 kjører flere replikaer, og sesjonen ligger i minnet på én pod (R11). Altinn-apper deler nøklene for databeskyttelse |
| 4 | Sette `SmartOnFhir:AllowedIssuerList` til `https://epj.ekstern.dev.nav.no/fhir` i TT02-innstillingene, og lese klienthemmelighet eller privatnøkkel fra Azure Key Vault | Utenfor Development avvises alle launcher når listen er tom. Privatnøkkelen leses i dag fra en filsti |
| 5 | Be NAV registrere klienten med TT02-adressene på `digdir.apps.tt02.altinn.no`, launch-modus `tab` og samme scopes som lokalt | Klienter registreres manuelt av NAV. `tab` trengs fordi informasjonskapsler og Altinn-innloggingen ikke virker i nav-epj sin iframe |
| 6 | Bekrefte at TT02-appen når `epj.ekstern.dev.nav.no` | Token-utvekslingen og FHIR-kallene går fra appens server. Ikke verifisert |

Et mellomsteg før TT02: be NAV om også å registrere en `local.altinn.cloud`-adresse. Redirecten skjer i nettleseren, så den lokale appen kan da bruke den driftede nav-epj, med ekte HelseID-innlogging og NAVs testdata.

---

## Kilder for hvert steg

| Steg i flyten | Detaljert beskrivelse |
|---|---|
| Pasient/Helsenorge | [PASIENTFLYT.md](PASIENTFLYT.md) — begge alternativer |
| Helsenorge EksternAPI teknisk | [IMPLEMENTERING.md §14.1](IMPLEMENTERING.md), [local-dev/helseid-token-test/](../local-dev/helseid-token-test/), [local-dev/helsenorge-oppgave-test/](../local-dev/helsenorge-oppgave-test/) |
| EPJ ↔ Altinn (SMART launch) | [KRAVSPESIFIKASJON-v0.6.md §4](KRAVSPESIFIKASJON-v0.6.md), [arkitektur-oversikt.svg](arkitektur-oversikt.svg), [smart-launch-sekvens.svg](smart-launch-sekvens.svg) |
| Testing mot ekte SMART-server + funn (redirect-bugs, sesjonsgap) | [IMPLEMENTERING.md §13](IMPLEMENTERING.md) |
| Signering | [IMPLEMENTERING.md](IMPLEMENTERING.md) — Altinn signing task-avsnittet |
| Altinn ↔ SVV (mottak) | [BESLUTNINGER.md C-3](BESLUTNINGER.md) — Altinn Events, FINT Arkiv-mønster, to-lags datamodell |
| Altinn ↔ EPJ (writeback) | [VEIKART.md fase 2](VEIKART.md) |
| Lokalt utviklingsmiljø (hvordan spinne opp alt) | [README.md «Kom i gang»](../README.md) |

---

## Hvordan spinne opp hele miljøet lokalt

Bekreftet fungerende 2026-08-11 (se README.md for fullstendige steg). Fire prosesser må kjøre samtidig:

> For en launch fra NAVs `nav-epj` i stedet for repoets egen mock, se [LOKALT-MED-NAV-EPJ.md](LOKALT-MED-NAV-EPJ.md).

| Komponent | Port | Kommando |
|---|---|---|
| Altinn Platform (localtest) + PDF + loadbalancer | 5101, 5300, 8000 | Podman/Podman Desktop, `app-localtest/docker-compose.yml` (separat repo, se README «Standarder og referanser») |
| HAPI FHIR (EPJ-mock) | 8080 | Del av samme compose-oppsett |
| SMART Auth Mock | 9090 | `cd local-dev/smart-mock && node server.js` |
| Altinn-appen (`forer-legeerklaering`) | 5005 | `cd src/App && dotnet run` |

**Kjente fallgruver:**
- Altinn localtest-plattformen (port 5101/8000) kan henge/ikke svare hvis den startes *før* Altinn-appen selv er oppe på port 5005 — den ser ut til å ha en avhengighet til appen ved oppstart av forespørsler. Start i denne rekkefølgen: containere → SMART mock (eller ingenting, se under) → Altinn-appen, og gi containerne litt tid før du tester.
- `app-localtest` sin nginx-konfig (`loadbalancer/templates/nginx.conf.conf`) bruker `proxy_set_header Host $host;` — dette **utelater porten** fra `Host`-headeren videre til appen, som ødelegger enhver funksjon som bygger egne redirect-URL-er fra `Request.Host` (bl.a. SMART-callback). Rettet lokalt til `$http_host`; husk `podman restart localtest-loadbalancer` etter endringen. Dette er en feil i Altinns delte infrastruktur, ikke i dette repoet — se [IMPLEMENTERING.md §13](IMPLEMENTERING.md).

Etter at alt kjører: åpne `http://localhost:9090/epj` for EPJ-simulatoren (anbefalt startpunkt for demo), eller gå direkte til `http://local.altinn.cloud:8000/digdir/forer-legeerklaering/smart/dev-login` for hurtigstart uten UI.

**For å teste den ekte SMART-launch-flyten** (ikke bare `/dev-login`) mot en standardkompatibel server, se [IMPLEMENTERING.md §13 «Teste mot launch.smarthealthit.org»](IMPLEMENTERING.md) — SMART Auth Mock trengs ikke for den testen, og hele kjeden inkl. Altinn-innlogging og FHIR-prefill er nå bekreftet fungerende (steg 2b i statustabellen over er dev-mitigert).
