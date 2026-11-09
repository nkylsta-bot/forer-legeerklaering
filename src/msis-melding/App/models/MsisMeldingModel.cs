using System.Text.Json.Serialization;
using System.Xml.Serialization;
using Newtonsoft.Json;

namespace Altinn.App.Models
{
    /// <summary>
    /// Nominativ melding om smittsom sykdom (MSIS-melding), etter FHIs papirskjema fastsatt
    /// september 2020. Alle felt er tekst. Datoer lagres som ÅÅÅÅ-MM-DD, og flervalg som
    /// kommaseparerte koder. Generert fra samme feltliste som MsisMelding.schema.json.
    /// </summary>
    [XmlRoot(ElementName = "MsisMelding")]
    public class MsisMeldingModel
    {
        [XmlElement("Pasient_Etternavn", Order = 1)]
        [JsonProperty("Pasient_Etternavn")]
        [JsonPropertyName("Pasient_Etternavn")]
        public string? Pasient_Etternavn { get; set; }

        [XmlElement("Pasient_Fornavn", Order = 2)]
        [JsonProperty("Pasient_Fornavn")]
        [JsonPropertyName("Pasient_Fornavn")]
        public string? Pasient_Fornavn { get; set; }

        [XmlElement("Pasient_Fnr", Order = 3)]
        [JsonProperty("Pasient_Fnr")]
        [JsonPropertyName("Pasient_Fnr")]
        public string? Pasient_Fnr { get; set; }

        [XmlElement("Pasient_Dnr", Order = 4)]
        [JsonProperty("Pasient_Dnr")]
        [JsonPropertyName("Pasient_Dnr")]
        public string? Pasient_Dnr { get; set; }

        [XmlElement("Pasient_Kjonn", Order = 5)]
        [JsonProperty("Pasient_Kjonn")]
        [JsonPropertyName("Pasient_Kjonn")]
        public string? Pasient_Kjonn { get; set; }

        [XmlElement("Pasient_Telefon", Order = 6)]
        [JsonProperty("Pasient_Telefon")]
        [JsonPropertyName("Pasient_Telefon")]
        public string? Pasient_Telefon { get; set; }

        [XmlElement("Pasient_Fastlege", Order = 7)]
        [JsonProperty("Pasient_Fastlege")]
        [JsonPropertyName("Pasient_Fastlege")]
        public string? Pasient_Fastlege { get; set; }

        [XmlElement("Pasient_Bokommune", Order = 8)]
        [JsonProperty("Pasient_Bokommune")]
        [JsonPropertyName("Pasient_Bokommune")]
        public string? Pasient_Bokommune { get; set; }

        [XmlElement("Pasient_Fodeland", Order = 9)]
        [JsonProperty("Pasient_Fodeland")]
        [JsonPropertyName("Pasient_Fodeland")]
        public string? Pasient_Fodeland { get; set; }

        [XmlElement("Pasient_MorsFodeland", Order = 10)]
        [JsonProperty("Pasient_MorsFodeland")]
        [JsonPropertyName("Pasient_MorsFodeland")]
        public string? Pasient_MorsFodeland { get; set; }

        [XmlElement("Pasient_FarsFodeland", Order = 11)]
        [JsonProperty("Pasient_FarsFodeland")]
        [JsonPropertyName("Pasient_FarsFodeland")]
        public string? Pasient_FarsFodeland { get; set; }

        [XmlElement("Pasient_BotidINorge", Order = 12)]
        [JsonProperty("Pasient_BotidINorge")]
        [JsonPropertyName("Pasient_BotidINorge")]
        public string? Pasient_BotidINorge { get; set; }

        [XmlElement("Pasient_AarsakOpphold", Order = 13)]
        [JsonProperty("Pasient_AarsakOpphold")]
        [JsonPropertyName("Pasient_AarsakOpphold")]
        public string? Pasient_AarsakOpphold { get; set; }

        [XmlElement("Pasient_YrkeInstitusjon", Order = 14)]
        [JsonProperty("Pasient_YrkeInstitusjon")]
        [JsonPropertyName("Pasient_YrkeInstitusjon")]
        public string? Pasient_YrkeInstitusjon { get; set; }

        [XmlElement("Epj_DiagnoseKode", Order = 15)]
        [JsonProperty("Epj_DiagnoseKode")]
        [JsonPropertyName("Epj_DiagnoseKode")]
        public string? Epj_DiagnoseKode { get; set; }

        [XmlElement("Epj_DiagnoseTekst", Order = 16)]
        [JsonProperty("Epj_DiagnoseTekst")]
        [JsonPropertyName("Epj_DiagnoseTekst")]
        public string? Epj_DiagnoseTekst { get; set; }

        [XmlElement("Sykdom", Order = 17)]
        [JsonProperty("Sykdom")]
        [JsonPropertyName("Sykdom")]
        public string? Sykdom { get; set; }

        [XmlElement("Sykdom_Innlagt", Order = 18)]
        [JsonProperty("Sykdom_Innlagt")]
        [JsonPropertyName("Sykdom_Innlagt")]
        public string? Sykdom_Innlagt { get; set; }

        [XmlElement("Sykdom_InnlagtType", Order = 19)]
        [JsonProperty("Sykdom_InnlagtType")]
        [JsonPropertyName("Sykdom_InnlagtType")]
        public string? Sykdom_InnlagtType { get; set; }

        [XmlElement("Sykdom_InnlagtDato", Order = 20)]
        [JsonProperty("Sykdom_InnlagtDato")]
        [JsonPropertyName("Sykdom_InnlagtDato")]
        public string? Sykdom_InnlagtDato { get; set; }

        [XmlElement("Sykdom_Institusjon", Order = 21)]
        [JsonProperty("Sykdom_Institusjon")]
        [JsonPropertyName("Sykdom_Institusjon")]
        public string? Sykdom_Institusjon { get; set; }

        [XmlElement("Sykdom_Baerertilstand", Order = 22)]
        [JsonProperty("Sykdom_Baerertilstand")]
        [JsonPropertyName("Sykdom_Baerertilstand")]
        public string? Sykdom_Baerertilstand { get; set; }

        [XmlElement("Sykdom_Innsykningsdato", Order = 23)]
        [JsonProperty("Sykdom_Innsykningsdato")]
        [JsonPropertyName("Sykdom_Innsykningsdato")]
        public string? Sykdom_Innsykningsdato { get; set; }

        [XmlElement("Sykdom_Provetakingsdato", Order = 24)]
        [JsonProperty("Sykdom_Provetakingsdato")]
        [JsonPropertyName("Sykdom_Provetakingsdato")]
        public string? Sykdom_Provetakingsdato { get; set; }

        [XmlElement("Sykdom_AntattSmittetidspunkt", Order = 25)]
        [JsonProperty("Sykdom_AntattSmittetidspunkt")]
        [JsonPropertyName("Sykdom_AntattSmittetidspunkt")]
        public string? Sykdom_AntattSmittetidspunkt { get; set; }

        [XmlElement("Sykdom_Laboratorium", Order = 26)]
        [JsonProperty("Sykdom_Laboratorium")]
        [JsonPropertyName("Sykdom_Laboratorium")]
        public string? Sykdom_Laboratorium { get; set; }

        [XmlElement("Sykdom_Indikasjon", Order = 27)]
        [JsonProperty("Sykdom_Indikasjon")]
        [JsonPropertyName("Sykdom_Indikasjon")]
        public string? Sykdom_Indikasjon { get; set; }

        [XmlElement("Sykdom_IndikasjonAnnet", Order = 28)]
        [JsonProperty("Sykdom_IndikasjonAnnet")]
        [JsonPropertyName("Sykdom_IndikasjonAnnet")]
        public string? Sykdom_IndikasjonAnnet { get; set; }

        [XmlElement("Sykdom_Sykdomsbilde", Order = 29)]
        [JsonProperty("Sykdom_Sykdomsbilde")]
        [JsonPropertyName("Sykdom_Sykdomsbilde")]
        public string? Sykdom_Sykdomsbilde { get; set; }

        [XmlElement("Sykdom_KliniskSepsis", Order = 30)]
        [JsonProperty("Sykdom_KliniskSepsis")]
        [JsonPropertyName("Sykdom_KliniskSepsis")]
        public string? Sykdom_KliniskSepsis { get; set; }

        [XmlElement("Sykdom_Utfall", Order = 31)]
        [JsonProperty("Sykdom_Utfall")]
        [JsonPropertyName("Sykdom_Utfall")]
        public string? Sykdom_Utfall { get; set; }

        [XmlElement("Sykdom_Dodsdato", Order = 32)]
        [JsonProperty("Sykdom_Dodsdato")]
        [JsonPropertyName("Sykdom_Dodsdato")]
        public string? Sykdom_Dodsdato { get; set; }

        [XmlElement("Sykdom_Folgetilstand", Order = 33)]
        [JsonProperty("Sykdom_Folgetilstand")]
        [JsonPropertyName("Sykdom_Folgetilstand")]
        public string? Sykdom_Folgetilstand { get; set; }

        [XmlElement("Sykdom_FolgetilstandDato", Order = 34)]
        [JsonProperty("Sykdom_FolgetilstandDato")]
        [JsonPropertyName("Sykdom_FolgetilstandDato")]
        public string? Sykdom_FolgetilstandDato { get; set; }

        [XmlElement("Smitte_Mate", Order = 35)]
        [JsonProperty("Smitte_Mate")]
        [JsonPropertyName("Smitte_Mate")]
        public string? Smitte_Mate { get; set; }

        [XmlElement("Smitte_Beskrivelse", Order = 36)]
        [JsonProperty("Smitte_Beskrivelse")]
        [JsonPropertyName("Smitte_Beskrivelse")]
        public string? Smitte_Beskrivelse { get; set; }

        [XmlElement("Smitte_Sted", Order = 37)]
        [JsonProperty("Smitte_Sted")]
        [JsonPropertyName("Smitte_Sted")]
        public string? Smitte_Sted { get; set; }

        [XmlElement("Smitte_Kommune", Order = 38)]
        [JsonProperty("Smitte_Kommune")]
        [JsonPropertyName("Smitte_Kommune")]
        public string? Smitte_Kommune { get; set; }

        [XmlElement("Smitte_Land", Order = 39)]
        [JsonProperty("Smitte_Land")]
        [JsonPropertyName("Smitte_Land")]
        public string? Smitte_Land { get; set; }

        [XmlElement("Smitte_Hjemkomstdato", Order = 40)]
        [JsonProperty("Smitte_Hjemkomstdato")]
        [JsonPropertyName("Smitte_Hjemkomstdato")]
        public string? Smitte_Hjemkomstdato { get; set; }

        [XmlElement("Smitte_UtenlandsAarsak", Order = 41)]
        [JsonProperty("Smitte_UtenlandsAarsak")]
        [JsonPropertyName("Smitte_UtenlandsAarsak")]
        public string? Smitte_UtenlandsAarsak { get; set; }

        [XmlElement("Smitte_Vaksinert", Order = 42)]
        [JsonProperty("Smitte_Vaksinert")]
        [JsonPropertyName("Smitte_Vaksinert")]
        public string? Smitte_Vaksinert { get; set; }

        [XmlElement("Covid_Naerkontakt", Order = 43)]
        [JsonProperty("Covid_Naerkontakt")]
        [JsonPropertyName("Covid_Naerkontakt")]
        public string? Covid_Naerkontakt { get; set; }

        [XmlElement("Covid_Eksponeringssted", Order = 44)]
        [JsonProperty("Covid_Eksponeringssted")]
        [JsonPropertyName("Covid_Eksponeringssted")]
        public string? Covid_Eksponeringssted { get; set; }

        [XmlElement("Smitte_UnderliggendeSykdom", Order = 45)]
        [JsonProperty("Smitte_UnderliggendeSykdom")]
        [JsonPropertyName("Smitte_UnderliggendeSykdom")]
        public string? Smitte_UnderliggendeSykdom { get; set; }

        [XmlElement("Hiv_MeldtTidligere", Order = 46)]
        [JsonProperty("Hiv_MeldtTidligere")]
        [JsonPropertyName("Hiv_MeldtTidligere")]
        public string? Hiv_MeldtTidligere { get; set; }

        [XmlElement("Hiv_Cd4", Order = 47)]
        [JsonProperty("Hiv_Cd4")]
        [JsonPropertyName("Hiv_Cd4")]
        public string? Hiv_Cd4 { get; set; }

        [XmlElement("Hiv_TidligerePositivProve", Order = 48)]
        [JsonProperty("Hiv_TidligerePositivProve")]
        [JsonPropertyName("Hiv_TidligerePositivProve")]
        public string? Hiv_TidligerePositivProve { get; set; }

        [XmlElement("Hiv_SisteNegativProve", Order = 49)]
        [JsonProperty("Hiv_SisteNegativProve")]
        [JsonPropertyName("Hiv_SisteNegativProve")]
        public string? Hiv_SisteNegativProve { get; set; }

        [XmlElement("Syfilis_Stadium", Order = 50)]
        [JsonProperty("Syfilis_Stadium")]
        [JsonPropertyName("Syfilis_Stadium")]
        public string? Syfilis_Stadium { get; set; }

        [XmlElement("Smitte_Veiledning", Order = 51)]
        [JsonProperty("Smitte_Veiledning")]
        [JsonPropertyName("Smitte_Veiledning")]
        public string? Smitte_Veiledning { get; set; }

        [XmlElement("Smitte_Tiltak", Order = 52)]
        [JsonProperty("Smitte_Tiltak")]
        [JsonPropertyName("Smitte_Tiltak")]
        public string? Smitte_Tiltak { get; set; }

        [XmlElement("Melder_Navn", Order = 53)]
        [JsonProperty("Melder_Navn")]
        [JsonPropertyName("Melder_Navn")]
        public string? Melder_Navn { get; set; }

        [XmlElement("Melder_Hpr", Order = 54)]
        [JsonProperty("Melder_Hpr")]
        [JsonPropertyName("Melder_Hpr")]
        public string? Melder_Hpr { get; set; }

        [XmlElement("Melder_Virksomhet", Order = 55)]
        [JsonProperty("Melder_Virksomhet")]
        [JsonPropertyName("Melder_Virksomhet")]
        public string? Melder_Virksomhet { get; set; }

        [XmlElement("Melder_Orgnr", Order = 56)]
        [JsonProperty("Melder_Orgnr")]
        [JsonPropertyName("Melder_Orgnr")]
        public string? Melder_Orgnr { get; set; }

        [XmlElement("Melder_Adresse", Order = 57)]
        [JsonProperty("Melder_Adresse")]
        [JsonPropertyName("Melder_Adresse")]
        public string? Melder_Adresse { get; set; }

        [XmlElement("Melder_Telefon", Order = 58)]
        [JsonProperty("Melder_Telefon")]
        [JsonPropertyName("Melder_Telefon")]
        public string? Melder_Telefon { get; set; }

        [XmlElement("Melder_Epost", Order = 59)]
        [JsonProperty("Melder_Epost")]
        [JsonPropertyName("Melder_Epost")]
        public string? Melder_Epost { get; set; }

        [XmlElement("Epj_KonsultasjonDato", Order = 60)]
        [JsonProperty("Epj_KonsultasjonDato")]
        [JsonPropertyName("Epj_KonsultasjonDato")]
        public string? Epj_KonsultasjonDato { get; set; }

        [XmlElement("Pasient_Fodselsdato", Order = 61)]
        [JsonProperty("Pasient_Fodselsdato")]
        [JsonPropertyName("Pasient_Fodselsdato")]
        public string? Pasient_Fodselsdato { get; set; }
    }
}
