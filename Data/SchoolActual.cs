using mapa_back.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace mapa_back.Data
{
    [Table("private_schools")]
    public class SchoolActual : School 
    {
		public SchoolActual() { }
		public SchoolActual(School source)
		{
			if (source == null) return;

			NumerRspo = source.NumerRspo;
			Geography = source.Geography;
			Typ = source.Typ;
			StatusPublicznoPrawny = source.StatusPublicznoPrawny;
			Nazwa = source.Nazwa;
			Wojewodztwo = source.Wojewodztwo;
			Gmina = source.Gmina;
			Powiat = source.Powiat;
			Miejscowosc = source.Miejscowosc;
			GminaRodzaj = source.GminaRodzaj;
			KodPocztowy = source.KodPocztowy;
			Ulica = source.Ulica;
			NumerBudynku = source.NumerBudynku;
			NumerLokalu = source.NumerLokalu;
			Email = source.Email;
			Telefon = source.Telefon;
			StronaInternetowa = source.StronaInternetowa;
			DyrektorImie = source.DyrektorImie;
			DyrektorNazwisko = source.DyrektorNazwisko;
			Nip = source.Nip;
			Regon = source.Regon;
			DataRozpoczecia = source.DataRozpoczecia;
			DataZalozenia = source.DataZalozenia;
			DataZakonczenia = source.DataZakonczenia;
			DataLikwidacji = source.DataLikwidacji;
			LiczbaUczniow = source.LiczbaUczniow;
			KategoriaUczniow = source.KategoriaUczniow;
			SpecyfikaSzkoly = source.SpecyfikaSzkoly;
			PodmiotProwadzacy = source.PodmiotProwadzacy;
			PodmiotProwadzacyTyp = source.PodmiotProwadzacyTyp;
		}
	}


}
