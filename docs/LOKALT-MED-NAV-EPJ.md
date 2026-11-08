# Kjør appen lokalt med nav-epj

Denne guiden setter opp hele kjeden på din egen maskin: NAVs test-EPJ [`navikt/nav-epj`](https://github.com/navikt/nav-epj) starter appen med SMART EHR Launch, og appen forhåndsutfyller legeerklæringen fra nav-epj sitt FHIR-API.

Guiden erstatter [NAV-EPJ-TESTMILJO.md](NAV-EPJ-TESTMILJO.md), som beskriver nav-epj slik den var i august 2026.

## Slik henger det sammen

| Del | Adresse |
|---|---|
| Altinn localtest og appen | `http://local.altinn.cloud:8000/digdir/forer-legeerklaering` |
| nav-epj backend, med SMART og FHIR | `http://localhost:8080` |
| nav-epj frontend, selve EPJ-et | `http://localhost:5173` |

Legen åpner appen fra nav-epj. nav-epj sender nettleseren til appens `/smart/launch`. Appen henter token og FHIR-data på serversiden og åpner skjemaet ferdig utfylt.

## Forutsetninger

- .NET 8 SDK
- [studioctl](https://docs.altinn.studio/) (Altinn Studio CLI)
- Podman eller Docker, med Compose
- Java 25
- Node 22 eller nyere, med `corepack enable`
- Et GitHub-token med tilgangen `read:packages`. nav-epj sin frontend henter NAVs designsystem fra GitHub Packages, som krever innlogging også for offentlige pakker.

Ikke kjør repoets gamle `smart-mock` eller HAPI FHIR-serveren samtidig. HAPI bruker port 8080, som nav-epj trenger.

## 1. Start Altinn localtest

```bash
studioctl env hosts add   # én gang, legger til local.altinn.cloud i hosts-filen
studioctl env up
```

Localtest svarer på `http://local.altinn.cloud:8000`. Får du en annen port, må du bruke den samme porten i klientregistreringen i steg 2.

## 2. Klon nav-epj og registrer appen

```bash
git clone https://github.com/navikt/nav-epj.git
cd nav-epj
```

Gjør to lokale endringer. Ikke commit dem.

1. **Registrer appen som SMART-klient.** Lim inn innholdet i [`local-dev/nav-epj-testing/forer-legeerklaering-client.yaml`](../local-dev/nav-epj-testing/forer-legeerklaering-client.yaml) nederst i `smart.clients` i `src/main/resources/application-local.yaml`, rett over linjen `valkey:`.
2. **Tillat `local.altinn.cloud` over http.** nav-epj godtar http-adresser bare for `localhost` og `127.0.0.1`. Localtest finner appen bare på `local.altinn.cloud`, som også peker til 127.0.0.1. Endre linje 8 i `src/main/kotlin/no/nav/helse/smart/security/SmartClientRegistry.kt` til:

   ```kotlin
   private val LOCAL_HOSTS = setOf("localhost", "127.0.0.1", "local.altinn.cloud")
   ```

## 3. Start nav-epj

Start databasene, og deretter backend i ett terminalvindu:

```bash
docker compose up -d
./gradlew runLocal        # Windows: .\gradlew.bat runLocal
```

Backend er klar når `http://localhost:8080/fhir/.well-known/smart-configuration` svarer. Lokalt er du alltid logget inn som legen GRØNN VITS, HPR-nummer 565501872, ved Fjordvik Legekontor.

Start frontend i et annet terminalvindu:

```bash
cd frontend
export NPM_AUTH_TOKEN=<ditt GitHub-token>     # PowerShell: $env:NPM_AUTH_TOKEN = "<ditt GitHub-token>"
yarn
yarn dev
```

All testdata slettes og legges inn på nytt hver gang backend starter.

## 4. Start appen

```bash
cd src/App
dotnet user-secrets set "SmartOnFhir:ClientSecret" "local-dev-secret"   # én gang
studioctl run
```

Hemmeligheten må være den samme som `clientSecret` i klientregistreringen fra steg 2.

## 5. Åpne appen fra nav-epj

1. Gå til `http://localhost:5173`.
2. Velg en pasient, for eksempel GLAD FJORD, og start en konsultasjon.
3. Åpne «Legeerklæring førerrett» fra applistene. Appen åpnes i en ny fane.

Skjemaet åpnes med pasient, lege, legekontor, konsultasjon og diagnose fylt ut.

**Uten frontend** kan du starte en launch direkte mot API-et. Det er nyttig hvis du ikke har et GitHub-token:

```bash
B=http://localhost:8080
P=$(curl -s $B/api/patient | jq -r '.[] | select(.givenName=="GLAD") | .id')
curl -s -X PUT $B/api/active-patient -H 'Content-Type: application/json' -d "{\"patientId\":\"$P\"}"
curl -s -X POST $B/api/patients/$P/consultations > /dev/null
curl -s -X POST $B/api/launch -H 'Content-Type: application/json' \
  -d "{\"appId\":\"forer-legeerklaering-poc\",\"patientId\":\"$P\"}" | jq -r .launchUrl
```

I PowerShell:

```powershell
$B = "http://localhost:8080"
$P = ((Invoke-RestMethod "$B/api/patient") | Where-Object givenName -eq "GLAD").id
Invoke-RestMethod -Method Put "$B/api/active-patient" -ContentType "application/json" -Body (@{ patientId = $P } | ConvertTo-Json) | Out-Null
Invoke-RestMethod -Method Post "$B/api/patients/$P/consultations" | Out-Null
(Invoke-RestMethod -Method Post "$B/api/launch" -ContentType "application/json" -Body (@{ appId = "forer-legeerklaering-poc"; patientId = $P } | ConvertTo-Json)).launchUrl
```

Åpne lenken som skrives ut, i nettleseren. Hver lenke virker én gang.

## Testscenarioer

| # | Scenario | Slik gjør du det | Forventet resultat |
|---|---|---|---|
| 1 | Forhåndsutfylling | Åpne appen for GLAD FJORD som i steg 5. | Fødselsnummer 30898110431, navn, fødselsdato og kjønn er fylt ut. Legen er GRØNN VITS med HPR-nummer 565501872, og legekontoret er Fjordvik Legekontor med organisasjonsnummer 311111116. Konsultasjonsdato og diagnose er også fylt ut. |
| 2 | Bytte pasient | Velg TRYGG FJORD i nav-epj, start en konsultasjon og åpne appen på nytt. | Et nytt skjema åpnes med TRYGG FJORD, fødselsnummer 02867910193. |
| 3 | Gjenbrukt launch | Åpne den samme launch-lenken en gang til, for eksempel fra nettleserhistorikken. | nav-epj avviser den, og appen viser «Authorization error: invalid_request». |
| 4 | Feil klienthemmelighet | Sett en annen verdi for `SmartOnFhir:ClientSecret`, start appen på nytt og åpne den fra nav-epj. | Appen viser «Token exchange failed». Sett tilbake `local-dev-secret` etterpå. |
| 5 | Sende inn | I skjemaet fra scenario 1 velger du en kjøretøygruppe, svarer «Ja, pasienten er skikket» og trykker «Send inn legeerklæring». | Innsendingen fullføres som før, med PDF og kvittering. |

## Når noe går galt

| Symptom | Årsak |
|---|---|
| nav-epj starter ikke og klager på «insecure redirectUri» | Endring 2 i steg 2 mangler. |
| `yarn` feiler med «Invalid authentication» | `NPM_AUTH_TOKEN` mangler eller har ikke `read:packages`. |
| Appen står ikke i applistene i nav-epj | Klientregistreringen er ikke lagt inn, eller backend er ikke startet på nytt etterpå. |
| Lege og legekontor er tomme, men pasienten er fylt ut | Appen fikk ikke validert id_token. Se etter «id_token» i appens logg med `studioctl app logs`. |
| `/smart/test-writeback` gir HTTP 400 | nav-epj godtar bare sitt eget notatformat for DocumentReference. Testen er skrevet for Epic. |
