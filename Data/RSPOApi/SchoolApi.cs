using System.Text.Json.Serialization;

namespace mapa_back.Models.RSPOApi
{
    public class SchoolApi
    {
        [JsonPropertyName("typ")]
        public RSPOTypeSchema? Typ { get; set; }

        [JsonPropertyName("email")]
        public string? Email { get; set; }

        [JsonPropertyName("gmina")]
        public string? Gmina { get; set; }

        [JsonPropertyName("nazwa")]
        public required string Nazwa { get; set; }

        [JsonPropertyName("ulica")]
        public string? Ulica { get; set; }

        [JsonPropertyName("powiat")]
        public string? Powiat { get; set; }

        [JsonPropertyName("telefon")]
        public string? Telefon { get; set; }

        [JsonPropertyName("dyrektorImie")]
        public string? DyrektorImie { get; set; } 

        [JsonPropertyName("dyrektorNazwisko")]
        public string? DyrektorNazwisko { get; set; } 

        [JsonPropertyName("numerRspo")]
        public required int NumerRspo { get; set; }

        [JsonPropertyName("kodPocztowy")]
        public string? KodPocztowy { get; set; }

        [JsonPropertyName("miejscowosc")]
        public string? Miejscowosc { get; set; }

        [JsonPropertyName("nip")]
        public string? Nip { get; set; }

        [JsonPropertyName("numerLokalu")]
        public string? NumerLokalu { get; set; }

        [JsonPropertyName("wojewodztwo")]
        public string? Wojewodztwo { get; set; }

        [JsonPropertyName("numerBudynku")]
        public string? NumerBudynku { get; set; }

        [JsonPropertyName("dataZalozenia")]
        public string? DataZalozenia { get; set; }

        [JsonPropertyName("liczbaUczniow")]
        public int? LiczbaUczniow { get; set; }

        [JsonPropertyName("regon")]
        public string? Regon { get; set; }

        [JsonPropertyName("dataZakonczenia")]
        public string? DataZakonczenia { get; set; }

        [JsonPropertyName("dataLikwidacji")]
        public string? DataLikwidacji { get; set; }

        [JsonPropertyName("kategoriaUczniow")]
        public RSPOTypeSchema? KategoriaUczniow { get; set; }

        [JsonPropertyName("specyfikaSzkoly")]
        public RSPOTypeSchema? SpecyfikaSzkoly { get; set; }

        [JsonPropertyName("statusPublicznoPrawny")]
        public RSPOTypeSchema? StatusPublicznoPrawny { get; set; }

        [JsonPropertyName("stronaInternetowa")]
        public string? StronaInternetowa { get; set; }

        [JsonPropertyName("gminaRodzaj")]
        public string? GminaRodzaj { get; set; }

        [JsonPropertyName("dataRozpoczecia")]
        public string? DataRozpoczecia { get; set; }

        [JsonPropertyName("podmiotProwadzacy")]
		public List<RSPOTypeSchema>? PodmiotProwadzacy { get; set; }

		[JsonPropertyName("geolokalizacja")]
		public required List<Geolokalizacja> Geolokalizacja { get; set; }
		public Geolokalizacja? Geo => Geolokalizacja?.FirstOrDefault();

	}
}
