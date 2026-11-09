using Altinn.App.Core.Models;

namespace Altinn.App.Services
{
    /// <summary>
    /// De meldingspliktige sykdommene i gruppe A som meldes på dette skjemaet, fra veiledningen på
    /// side 2 i FHIs papirskjema (september 2020). Tuberkulose meldes på egne skjemaer og er ikke med.
    ///
    /// Hver sykdom har ICD-10-kodene den svarer til, slik at skjemaet kan foreslå sykdommen fra
    /// diagnosen i journalen. Kodene er et utvalg for PoC-en, ikke en offisiell kodeliste fra FHI.
    /// </summary>
    public static class MsisSykdommer
    {
        public sealed record Sykdom(string Kode, string Navn, bool SkalVarsles, params string[] Icd10);

        public static readonly IReadOnlyList<Sykdom> Alle =
        [
            new("difteri", "Difteri", true, "A36"),
            new("kikhoste", "Kikhoste", false, "A37"),
            new("kusma", "Kusma", false, "B26"),
            new("meslinger", "Meslinger", true, "B05"),
            new("poliomyelitt", "Poliomyelitt", true, "A80"),
            new("rotavirus", "Rotavirus", false, "A08.0"),
            new("rode-hunder", "Røde hunder", true, "B06"),
            new("haemophilus", "Systemisk Haemophilus influenzae-sykdom", false, "A41.3", "G00.0"),
            new("tetanus", "Tetanus (stivkrampe)", false, "A33", "A34", "A35"),
            new("hepatitt-a", "Hepatitt A", false, "B15"),
            new("hepatitt-b", "Hepatitt B", false, "B16", "B18.0", "B18.1"),
            new("hepatitt-c", "Hepatitt C", false, "B17.1", "B18.2"),
            new("botulisme", "Botulisme", true, "A05.1"),
            new("campylobacteriose", "Campylobacteriose", false, "A04.5"),
            new("hus", "Diaréassosiert hemolytisk uremisk syndrom", true, "D59.3"),
            new("e-coli", "E. coli-enteritt", false, "A04.0", "A04.1", "A04.2", "A04.3", "A04.4"),
            new("giardiasis", "Giardiasis", false, "A07.1"),
            new("listeriose", "Listeriose", false, "A32"),
            new("salmonellose", "Salmonellose", false, "A02"),
            new("yersiniose", "Yersiniose", false, "A04.6"),
            new("cryptosporidiose", "Cryptosporidiose", false, "A07.2"),
            new("brucellose", "Brucellose", false, "A23"),
            new("ekinokokkose", "Ekinokokkose", false, "B67"),
            new("lyme-borreliose", "Lyme borreliose", false, "A69.2"),
            new("miltbrann", "Miltbrann", true, "A22"),
            new("nephropathia-epidemica", "Nephropathia epidemica", false, "A98.5"),
            new("rabies", "Rabies", true, "A82"),
            new("trikinose", "Trikinose", true, "B75"),
            new("tularemi", "Tularemi", false, "A21"),
            new("denguefeber", "Denguefeber", false, "A90", "A91"),
            new("flekktyfus", "Flekktyfus", true, "A75"),
            new("gulfeber", "Gulfeber", false, "A95"),
            new("viral-hemoragisk-feber", "Viral hemoragisk feber", true, "A96", "A98.0", "A98.3", "A98.4", "A99"),
            new("kolera", "Kolera", true, "A00"),
            new("lepra", "Lepra", false, "A30"),
            new("malaria", "Malaria", false, "B50", "B51", "B52", "B53", "B54"),
            new("pest", "Pest", true, "A20"),
            new("q-feber", "Q-feber", false, "A78"),
            new("shigellose", "Shigellose", false, "A03"),
            new("tilbakefallsfeber", "Tilbakefallsfeber", false, "A68"),
            new("vestnilfeber", "Vestnilfeber", false, "A92.3"),
            new("zikafeber", "Zikafeber-infeksjon", false, "A92.5"),
            new("legionellose", "Legionellose", true, "A48.1"),
            new("aids", "Aids", false),
            new("sars", "Alvorlig, akutt luftveissyndrom – sars", true, "U04"),
            new("tuberkulose-hiv", "Dobbelinfeksjon med tuberkulose og hiv", false),
            new("pandemisk-influensa", "Influensa forårsaket av virus med pandemisk potensial", false, "J09"),
            new("koronavirus", "Koronavirus med utbruddspotensial (inkl. covid-19)", false, "U07.1", "U07.2"),
            new("mers", "Middle East Respiratory Syndrome – mers", true),
            new("kopper", "Kopper", true, "B03"),
            new("paratyfoidfeber", "Paratyfoidfeber", false, "A01.1", "A01.2", "A01.3", "A01.4"),
            new("prionsykdommer", "Prionsykdommer", false, "A81"),
            new("meningokokk", "Systemisk meningokokksykdom", true, "A39"),
            new("pneumokokk", "Systemisk pneumokokksykdom", false, "G00.1", "A40.3"),
            new("streptokokk-a", "Systemisk gruppe A-streptokokksykdom", false, "A40.0"),
            new("streptokokk-b", "Systemisk gruppe B-streptokokksykdom", false, "A40.1"),
            new("tyfoidfeber", "Tyfoidfeber", false, "A01.0"),
            new("virale-cns-infeksjoner", "Virale infeksjoner i sentralnervesystemet", false, "A84", "A85", "A86"),
            new(
                "resistente-mikrober",
                "Sykdom forårsaket av visse resistente mikrober (MRSA, PRP, C. difficile, VRE m.fl.)",
                false
            ),
            new("lgv", "Lymfogranuloma venereum (LGV)", false, "A55"),
            new("gonore", "Gonoré", false, "A54"),
            new("hiv-infeksjon", "Hiv-infeksjon", false, "B20", "B21", "B22", "B23", "B24", "Z21"),
            new("syfilis", "Syfilis", false, "A50", "A51", "A52", "A53"),
        ];

        public static Sykdom? MedKode(string? kode) => Alle.FirstOrDefault(s => s.Kode == kode);

        /// <summary>
        /// Sykdommen en ICD-10-kode peker på, eller null. Den lengste koden som er et prefiks vinner,
        /// så A01.0 gir tyfoidfeber og A01.1 paratyfoidfeber.
        /// </summary>
        public static Sykdom? FraIcd10(string? icd10)
        {
            if (string.IsNullOrWhiteSpace(icd10))
                return null;
            var kode = icd10.Trim().ToUpperInvariant();
            return Alle.SelectMany(s => s.Icd10.Select(prefix => (Sykdom: s, Prefix: prefix)))
                .Where(x => kode.StartsWith(x.Prefix, StringComparison.Ordinal))
                .OrderByDescending(x => x.Prefix.Length)
                .Select(x => x.Sykdom)
                .FirstOrDefault();
        }
    }

    /// <summary>Kodelisten «sykdommer» i skjemaet, med varslingspliktige sykdommer merket.</summary>
    public sealed class SykdomOptionsProvider : IAppOptionsProvider
    {
        public string Id => "sykdommer";

        public Task<AppOptions> GetAppOptions(string? language, Dictionary<string, string> keyValuePairs) =>
            Task.FromResult(
                new AppOptions
                {
                    IsCacheable = true,
                    Options = MsisSykdommer
                        .Alle.Select(s => new AppOption
                        {
                            Value = s.Kode,
                            Label = s.SkalVarsles ? $"{s.Navn} (skal også varsles)" : s.Navn,
                        })
                        .ToList(),
                }
            );
    }
}
