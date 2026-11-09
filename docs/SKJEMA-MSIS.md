# MSIS-melding om smittsom sykdom — skjemastruktur og forhåndsutfylling

## Nominativ melding om smittsom sykdom (FHI, fastsatt september 2020)

**Kilde:** [MSIS-meldeskjema, nominativ melding om smittsom sykdom (PDF)](https://www.fhi.no/globalassets/dokumenterfiler/skjema/msis/msis-meldeskjema_nominativ-melding-om-smittsom-sykdom.pdf)
**Hjemmel:** MSIS-forskriften, med hjemmel i helseregisterloven og smittevernloven.
**Formål:** Legen melder sykdommer i gruppe A til MSIS ved Folkehelseinstituttet samme dag som sykdommen er oppdaget eller mistenkt. Papirskjemaet sier også at en kopi skal lagres i pasientens journal. Tuberkulose meldes på egne skjemaer og er ikke med.

**Lagt til 2026-11-09** som tredje skjema i prosjektet, bygd på `forer-legeerklaering` (Altinn app v9). Appen ligger i `src/msis-melding/App`, med `src/msis-melding` som approt, slik `studioctl` forventer.

---

## Slik er skjemaet bygd opp

Papirskjemaet har to kolonner på én side. Appen deler det i fire sider:

| Side | Innhold | Fra journalen |
|---|---|---|
| Pasient | Personopplysninger, fødeland, botid og yrke/institusjon | Navn, fødselsnummer eller D-nummer, kjønn |
| Sykdom | Meldingspliktig sykdom, innleggelse, datoer, laboratorium, indikasjon, sykdomsbilde og utfall | ICD-10-diagnosen, og et forslag til sykdom |
| Smitte | Smittemåte, smittested, utenlandsopphold, vaksinasjon, og egne seksjoner for covid-19, hiv og syfilis | – |
| Melder | Lege, legekontor og kontaktopplysninger | Legens navn og HPR-nummer, legekontorets navn og organisasjonsnummer |

Alle feltene i datamodellen `MsisMeldingModel` er tekst. Datoer lagres som ÅÅÅÅ-MM-DD, og flervalg som kommaseparerte koder.

**Felt som bare vises når de er aktuelle:**

- Hvor pasienten er innlagt, dato og institusjon vises når «Innlagt helseinstitusjon?» er «Ja».
- Dødsdato vises når utfallet er «Død».
- Kommune vises ved smitte i Norge. Land, hjemkomstdato og årsak til utenlandsopphold vises ved smitte i utlandet.
- Seksjonen for covid-19 vises for «Koronavirus med utbruddspotensial», seksjonen for hiv for hiv-infeksjon, aids og dobbelinfeksjon med tuberkulose, og seksjonen for syfilis for syfilis.

## Forhåndsutfylling

`MsisPrefillService` bruker samme SMART-launch og FHIR-henting som førerrett-appen (`SmartFhir.Common`).

| Felt i meldingen | FHIR-kilde | Kan legen endre det? |
|---|---|---|
| Etternavn, fornavn | `Patient.name` | Nei |
| Fødselsnummer | `Patient.identifier`, OID `2.16.578.1.12.4.1.4.1` | Nei |
| D-nummer | `Patient.identifier`, OID `2.16.578.1.12.4.1.4.2` | Nei |
| Kjønn | `Patient.gender` | Ja, fylles bare når feltet er tomt |
| Diagnose i journalen | Første `Condition` på konsultasjonen kodet i ICD-10, OID `2.16.578.1.12.4.1.1.7110` | Nei |
| Meldingspliktig sykdom | Foreslått fra ICD-10-koden | Ja, fylles bare når feltet er tomt |
| Melders navn og HPR-nummer | `Practitioner`, fra `fhirUser` i id_token | Nei |
| Legekontor og organisasjonsnummer | `PractitionerRole` → `Organization` | Nei |

**Forslaget til sykdom** kommer fra ICD-10-kodene i `MsisSykdommer`, for eksempel A37 for kikhoste og B05 for meslinger. Den lengste koden som passer, vinner, så A01.0 gir tyfoidfeber og A01.1 paratyfoidfeber. Kodene er et utvalg for PoC-en, ikke en offisiell kodeliste fra FHI. Sykdommer som også skal varsles muntlig, er merket «(skal også varsles)» i listen.

**Diagnoser i nav-epj:** de innebygde konsultasjonene har én ICPC-2-hoveddiagnose og ingen ICD-10-kode. For å teste forslaget må konsultasjonen få en ICD-10-diagnose, se scenarioene i [LOKALT-MED-NAV-EPJ.md](LOKALT-MED-NAV-EPJ.md).

## Kopi i journalen

Papirskjemaet skal lagres i pasientens journal. Etter innsending skriver `EpjWritebackServiceTask` PDF-en tilbake til EPJ-et som et journalnotat på konsultasjonen, på samme måte som førerrett-appen. Notatet sier hvilken sykdom som er meldt, referansen i Altinn, innsykningsdato og smitteverntiltak.

## Det som ikke er gjort

- Meldingen sendes ikke videre til MSIS, og kopien til kommuneoverlegen sendes ikke. Altinn-instansen er mottaket i PoC-en.
- Kommunelegen varsles ikke for sykdommene som skal varsles. Listen merker dem bare.
- Pasientens telefonnummer, fastlege og bokommune hentes ikke fra journalen. nav-epj har ikke disse opplysningene i FHIR.
- Bare bokmål.
