using mapa_back.Data.RSPOApi.PodmiotProwadzacy;

namespace mapa_back.Models.DTO
{
    public class SchoolDTO
    {
		public int Id { get; set; }
		public int NumerRspo { get; set; }
		public string? Typ { get; set; }
		public string? StatusPublicznoPrawny { get; set; }
		public string Nazwa { get; set; }
		public string? Wojewodztwo { get; set; }
		public string? Gmina { get; set; }
		public string? Powiat { get; set; }
		public string? Miejscowosc { get; set; }
		public string? GminaRodzaj { get; set; }
		public string? KodPocztowy { get; set; }
		public string? Ulica { get; set; }
		public string? NumerBudynku { get; set; }
		public string? NumerLokalu { get; set; }
		public string? Email { get; set; }
		public string? Telefon { get; set; }
		public string? StronaInternetowa { get; set; }
		public string? DyrektorImie { get; set; }
        public string? DyrektorNazwisko { get; set; }
		public string? Nip { get; set; }
		public string? Regon { get; set; }
		public DateOnly? DataRozpoczecia { get; set; }
		public DateOnly? DataZalozenia { get; set; }
        public DateOnly? DataZakonczenia { get; set; }
        public DateOnly? DataLikwidacji { get; set; }
		public int? LiczbaUczniow { get; set; }
		public string? KategoriaUczniow { get; set; }
		public string? SpecyfikaSzkoly { get; set; }
		public string? PodmiotProwadzacyTyp { get; set; }
        public string? PodmiotProwadzacyNazwa { get; set; }
        public GeographyDTO? Geography { get; set; }
    }
}
