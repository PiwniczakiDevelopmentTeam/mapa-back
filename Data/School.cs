using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using NetTopologySuite.Geometries;

namespace mapa_back.Models
{
	public class School
    {
        [Key]
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Required]
        public int Id { get; set; }

        [Column("numer_rspo")]
        [Required]
        public int NumerRspo { get; set; }

        [Column("geography")]
        [Required]
        public Point Geography { get; set; }

        [Column("typ")]
        [MaxLength(128)]
        public string? Typ { get; set; }

        [Column("status_publiczno_prawny")]
        [MaxLength(128)]
        public string? StatusPublicznoPrawny { get; set; }

        [Column("nazwa")]
        [MaxLength(256)]
        [Required]
        public string Nazwa { get; set; }

        [Column("wojewodztwo")]
        [MaxLength(32)]
        public string? Wojewodztwo { get; set; }

        [Column("gmina")]
        [MaxLength(64)]
        public string? Gmina { get; set; }

        [Column("powiat")]
        [MaxLength(32)]
        public string? Powiat { get; set; }

        [Column("miejscowosc")]
        [MaxLength(64)]
        public string? Miejscowosc { get; set; }

        [Column("gmina_rodzaj")]
        [MaxLength(32)]
        public string? GminaRodzaj { get; set; }

        [Column("kod_pocztowy")]
        [MaxLength(16)]
        public string? KodPocztowy { get; set; }

        [Column("ulica")]
        [MaxLength(128)]
        public string? Ulica { get; set; }

        [Column("numer_budynku")]
        [MaxLength(32)]
        public string? NumerBudynku { get; set; }

        [Column("numer_lokalu")]
        [MaxLength(32)]
        public string? NumerLokalu { get; set; }

        [Column("email")]
        [MaxLength(128)]
        public string? Email { get; set; }

        [Column("telefon")]
        [MaxLength(16)]
        public string? Telefon { get; set; }

        [Column("strona_internetowa")]
        [MaxLength(256)]
        public string? StronaInternetowa { get; set; }

        [Column("dyrektor_imie")]
        [MaxLength(32)]
        public string? DyrektorImie { get; set; }

        [Column("dyrektor_nazwisko")]
        [MaxLength(32)]
        public string? DyrektorNazwisko { get; set; }

        [Column("nip")]
        [MaxLength(10)]
        public string? Nip { get; set; }

        [Column("regon")]
        [MaxLength(14)]
        public string? Regon { get; set; }

        [Column("data_rozpoczecia")]
        public DateOnly? DataRozpoczecia { get; set; }

        [Column("data_zalozenia")]
        public DateOnly? DataZalozenia { get; set; }

        [Column("data_zakonczenia")]
        public DateOnly? DataZakonczenia { get; set; }

        [Column("data_likwidacji")]
        public DateOnly? DataLikwidacji { get; set; }

        [Column("liczba_uczniow")]
        public int? LiczbaUczniow { get; set; }

        [Column("kategoria_uczniow")]
        [MaxLength(64)]
        public string? KategoriaUczniow { get; set; }

        [Column("specyfika_szkoly")]
        [MaxLength(64)]
        public string? SpecyfikaSzkoly { get; set; }

        [Column("podmiot_prowadzacy")]
        public string? PodmiotProwadzacy { get; set; }

		[Column("podmiot_prowadzacy_typ")]
		public string? PodmiotProwadzacyTyp { get; set; }

		public override bool Equals(object? obj)
		{
			if (obj is not School other) return false;

			return NumerRspo == other.NumerRspo &&
				   (Geography?.EqualsExact(other.Geography) ?? other.Geography is null) &&
				   Typ == other.Typ &&
				   StatusPublicznoPrawny == other.StatusPublicznoPrawny &&
				   Nazwa == other.Nazwa &&
				   Wojewodztwo == other.Wojewodztwo &&
				   Gmina == other.Gmina &&
				   Powiat == other.Powiat &&
				   Miejscowosc == other.Miejscowosc &&
				   GminaRodzaj == other.GminaRodzaj &&
				   KodPocztowy == other.KodPocztowy &&
				   Ulica == other.Ulica &&
				   NumerBudynku == other.NumerBudynku &&
				   NumerLokalu == other.NumerLokalu &&
				   Email == other.Email &&
				   Telefon == other.Telefon &&
				   StronaInternetowa == other.StronaInternetowa &&
				   DyrektorImie == other.DyrektorImie &&
				   DyrektorNazwisko == other.DyrektorNazwisko &&
				   Nip == other.Nip &&
				   Regon == other.Regon &&
				   DataRozpoczecia == other.DataRozpoczecia &&
				   DataZalozenia == other.DataZalozenia &&
				   DataZakonczenia == other.DataZakonczenia &&
				   DataLikwidacji == other.DataLikwidacji &&
				   LiczbaUczniow == other.LiczbaUczniow &&
				   KategoriaUczniow == other.KategoriaUczniow &&
				   SpecyfikaSzkoly == other.SpecyfikaSzkoly &&
				   PodmiotProwadzacy == other.PodmiotProwadzacy &&
				   PodmiotProwadzacyTyp == other.PodmiotProwadzacyTyp;
		}

		public override int GetHashCode()
		{
			return NumerRspo.GetHashCode();
		}

	}

}
