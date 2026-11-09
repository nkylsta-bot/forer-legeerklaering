namespace SmartFhir.Common.Session
{
    /// <summary>
    /// Keys for the SMART launch context the app stores in the Altinn instance's data values. Only
    /// the app can set data values, so the user cannot point a later write-back at another patient.
    /// Service tasks read them, because they run without the clinician's session.
    /// </summary>
    public static class SmartInstanceDataValues
    {
        public const string FhirBaseUrl = "smart.fhirBaseUrl";
        public const string PatientId = "smart.patientId";
        public const string EncounterId = "smart.encounterId";
    }
}
