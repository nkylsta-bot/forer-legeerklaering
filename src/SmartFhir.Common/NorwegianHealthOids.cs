namespace SmartFhir.Common
{
    /// <summary>
    /// OID-er for identifikator-systemer som går igjen på tvers av alle norske helse-FHIR-caser
    /// (uavhengig av hvilket EPJ-system som er kilde). Flyttet hit fra FhirPrefillService slik at
    /// nye apper i dette repoet ikke må slå opp/gjette disse på nytt.
    ///
    /// HprOid gjelder ALL autorisert helsepersonell (leger, tannleger, tannpleiere osv.) —
    /// bekreftet ved kjeveortopedisk-caset 2026-09-09, som bruker samme OID for henvisende
    /// tannlege/tannpleier som ForerLegeerklaering bruker for lege.
    /// </summary>
    public static class NorwegianHealthOids
    {
        /// <summary>Fødselsnummer (FHIR Patient.identifier.system).</summary>
        public const string Fnr = "urn:oid:2.16.578.1.12.4.1.4.1";

        /// <summary>D-nummer (FHIR Patient.identifier.system).</summary>
        public const string Dnr = "urn:oid:2.16.578.1.12.4.1.4.2";

        /// <summary>ICD-10 (FHIR Condition.code.coding.system), Volven 7110.</summary>
        public const string Icd10 = "urn:oid:2.16.578.1.12.4.1.1.7110";

        /// <summary>HPR-nummer — gjelder all autorisert helsepersonell, ikke bare leger.</summary>
        public const string Hpr = "urn:oid:2.16.578.1.12.4.1.4.4";

        /// <summary>Organisasjonsnummer (FHIR Organization.identifier.system).</summary>
        public const string Orgnr = "urn:oid:2.16.578.1.12.4.1.4.101";

        /// <summary>HER-id (adresseregisteret) (FHIR Organization.identifier.system).</summary>
        public const string HerId = "urn:oid:2.16.578.1.12.4.1.2";
    }
}
