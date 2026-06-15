using mapa_back.Data;
using mapa_back.Data.RSPOApi.PodmiotProwadzacy;
using mapa_back.Models;
using mapa_back.Models.DTO;
using NetTopologySuite.Geometries;
using Newtonsoft.Json;
using System.Text.Json;

namespace mapa_back.Mappers
{
	public class SchoolMapper
	{
		public static SchoolDTO MapToDTO(SchoolFromRSPO school)
		{
			return new SchoolDTO
			{
				Id = school.Id,
				NumerRspo = school.NumerRspo,
				Typ = school.Typ,
				StatusPublicznoPrawny = school.StatusPublicznoPrawny,
				Nazwa = school.Nazwa,
				Wojewodztwo = school.Wojewodztwo,
				Gmina = school.Gmina,
				Powiat = school.Powiat,
				Miejscowosc = school.Miejscowosc,
				GminaRodzaj = school.GminaRodzaj,
				KodPocztowy = school.KodPocztowy,
				Ulica = school.Ulica,
				NumerBudynku = school.NumerBudynku,
				NumerLokalu = school.NumerLokalu,
				Email = school.Email,
				Telefon = school.Telefon,
				StronaInternetowa = school.StronaInternetowa,
				DyrektorImie = school.DyrektorImie,
				DyrektorNazwisko = school.DyrektorNazwisko,
				Nip = school.Nip,
				Regon = school.Regon,
				DataRozpoczecia = school.DataRozpoczecia,
				DataZalozenia = school.DataZalozenia,
				DataZakonczenia = school.DataZakonczenia,
				DataLikwidacji = school.DataLikwidacji,
				LiczbaUczniow = school.LiczbaUczniow,
				KategoriaUczniow = school.KategoriaUczniow,
				SpecyfikaSzkoly = school.SpecyfikaSzkoly,
				PodmiotProwadzacyTyp = school.PodmiotProwadzacyTyp,
				PodmiotProwadzacyNazwa = school.PodmiotProwadzacy,
				Geography = MapGeographyToDTO(school.Geography)
			};
		}

		public static SchoolDTO MapToDTO(School school)
		{
			return new SchoolDTO
			{
				Id = school.Id,
				NumerRspo = school.NumerRspo,
				Typ = school.Typ,
				StatusPublicznoPrawny = school.StatusPublicznoPrawny,
				Nazwa = school.Nazwa,
				Wojewodztwo = school.Wojewodztwo,
				Gmina = school.Gmina,
				Powiat = school.Powiat,
				Miejscowosc = school.Miejscowosc,
				GminaRodzaj = school.GminaRodzaj,
				KodPocztowy = school.KodPocztowy,
				Ulica = school.Ulica,
				NumerBudynku = school.NumerBudynku,
				NumerLokalu = school.NumerLokalu,
				Email = school.Email,
				Telefon = school.Telefon,
				StronaInternetowa = school.StronaInternetowa,
				DyrektorImie = school.DyrektorImie,
				DyrektorNazwisko = school.DyrektorNazwisko,
				Nip = school.Nip,
				Regon = school.Regon,
				DataRozpoczecia = school.DataRozpoczecia,
				DataZalozenia = school.DataZalozenia,
				DataZakonczenia = school.DataZakonczenia,
				DataLikwidacji = school.DataLikwidacji,
				LiczbaUczniow = school.LiczbaUczniow,
				KategoriaUczniow = school.KategoriaUczniow,
				SpecyfikaSzkoly = school.SpecyfikaSzkoly,
				PodmiotProwadzacyTyp = school.PodmiotProwadzacyTyp,
				PodmiotProwadzacyNazwa = school.PodmiotProwadzacy,
				Geography = MapGeographyToDTO(school.Geography),
			};
		}
		public static SchoolDTO MapToDTO(SchoolActual school)
		{
			return new SchoolDTO
			{
				Id = school.Id,
				NumerRspo = school.NumerRspo,
				Typ = school.Typ,
				StatusPublicznoPrawny = school.StatusPublicznoPrawny,
				Nazwa = school.Nazwa,
				Wojewodztwo = school.Wojewodztwo,
				Gmina = school.Gmina,
				Powiat = school.Powiat,
				Miejscowosc = school.Miejscowosc,
				GminaRodzaj = school.GminaRodzaj,
				KodPocztowy = school.KodPocztowy,
				Ulica = school.Ulica,
				NumerBudynku = school.NumerBudynku,
				NumerLokalu = school.NumerLokalu,
				Email = school.Email,
				Telefon = school.Telefon,
				StronaInternetowa = school.StronaInternetowa,
				DyrektorImie = school.DyrektorImie,
				DyrektorNazwisko = school.DyrektorNazwisko,
				Nip = school.Nip,
				Regon = school.Regon,
				DataRozpoczecia = school.DataRozpoczecia,
				DataZalozenia = school.DataZalozenia,
				DataZakonczenia = school.DataZakonczenia,
				DataLikwidacji = school.DataLikwidacji,
				LiczbaUczniow = school.LiczbaUczniow,
				KategoriaUczniow = school.KategoriaUczniow,
				SpecyfikaSzkoly = school.SpecyfikaSzkoly,
				PodmiotProwadzacyTyp = school.PodmiotProwadzacyTyp,
				PodmiotProwadzacyNazwa = school.PodmiotProwadzacy,
				Geography = MapGeographyToDTO(school.Geography),
				AutoUpdate = school.AutoUpdate,
			};
		}
		public static object MapToMapSchoolFormat(School school)
		{
			var lat = school.Geography?.Y ?? 0;
			var lon = school.Geography?.X ?? 0;

			return new
			{
				id = school.Id,
				latitude = lat,
				longtitude = lon, // matches "longtitude" in script.js
				businessData = new
				{
					nazwa = school.Nazwa ?? string.Empty,
					miejscowosc = school.Miejscowosc ?? string.Empty,
					kodPocztowy = school.KodPocztowy ?? string.Empty,
					poczta = school.Miejscowosc ?? string.Empty,
					wojewodztwo = school.Wojewodztwo ?? string.Empty,
					powiat = school.Powiat ?? string.Empty,
					gmina = school.Gmina ?? string.Empty,
					ulica = school.Ulica ?? string.Empty,
					numerBudynku = school.NumerBudynku ?? string.Empty,
					numerLokalu = school.NumerLokalu ?? string.Empty,
					numerIokalu = school.NumerLokalu ?? string.Empty, // Duplicate for the typo in script.js line 240
					dyrektor = $"{school.DyrektorImie} {school.DyrektorNazwisko}".Trim(),
					telefon = school.Telefon ?? string.Empty,
					faks = string.Empty,
					email = school.Email ?? string.Empty,
					stronaInternetowa = school.StronaInternetowa ?? string.Empty,
					typ = school.Typ ?? string.Empty,
					kategoriaUczniow = school.KategoriaUczniow ?? string.Empty,
					statusPublicznosc = school.StatusPublicznoPrawny ?? string.Empty,
					liczbaUczniow = school.LiczbaUczniow ?? 0,
					jezykiNauczane = Array.Empty<string>(),
					terenySportowe = string.Empty,
					strukturaMiejsce = string.Empty,
					rodzajMiejscowosci = school.GminaRodzaj ?? string.Empty,
					specyfikaPlacowki = school.SpecyfikaSzkoly ?? string.Empty,
					rspoNumer = school.NumerRspo.ToString(),
					regonPodmiotu = school.Regon ?? string.Empty,
					nipPodmiotu = school.Nip ?? string.Empty,
					dataRozpoczeciaDzialalnosci = school.DataRozpoczecia?.ToString("yyyy-MM-dd") ?? string.Empty,
					dataLikwidacji = school.DataLikwidacji?.ToString("yyyy-MM-dd") ?? string.Empty,
					kodTerytorialnyMiejscowosc = string.Empty,
					kodTerytorialnyGmina = string.Empty,
					kodTerytorialnyPowiat = string.Empty,
					kodTerytorialnyWojewodztwo = string.Empty,
					podmiotNadrzednyNazwa = string.Empty,
					podmiotNadrzednyTyp = string.Empty,
					podmiotNadrzednyRspo = string.Empty,
					organProwadzacyNazwa = school.PodmiotProwadzacy ?? string.Empty,
					organProwadzacyNip = string.Empty,
					organProwadzacyRegon = string.Empty,
					organProwadzacyTyp = school.PodmiotProwadzacyTyp ?? string.Empty,
					organProwadzacyGmina = string.Empty,
					organProwadzacyPowiat = string.Empty,
					organProwadzacyWojewodztwo = string.Empty
				}
			};
		}
		private static GeographyDTO? MapGeographyToDTO(Point geography)
		{
			if (geography == null) return null;

			return new GeographyDTO
			{
				Y = geography.Y,
				X = geography.X
			};
		}

		public static SchoolActual MapToActualSchool(SchoolDTO school)
		{
			return new SchoolActual
			{
				Id = school.Id,
				NumerRspo = school.NumerRspo,
				Typ = school.Typ,
				StatusPublicznoPrawny = school.StatusPublicznoPrawny,
				Nazwa = school.Nazwa,
				Wojewodztwo = school.Wojewodztwo,
				Gmina = school.Gmina,
				Powiat = school.Powiat,
				Miejscowosc = school.Miejscowosc,
				GminaRodzaj = school.GminaRodzaj,
				KodPocztowy = school.KodPocztowy,
				Ulica = school.Ulica,
				NumerBudynku = school.NumerBudynku,
				NumerLokalu = school.NumerLokalu,
				Email = school.Email,
				Telefon = school.Telefon,
				StronaInternetowa = school.StronaInternetowa,
				DyrektorImie = school.DyrektorImie,
				DyrektorNazwisko = school.DyrektorNazwisko,
				Nip = school.Nip,
				Regon = school.Regon,
				DataRozpoczecia = school.DataRozpoczecia,
				DataZalozenia = school.DataZalozenia,
				DataZakonczenia = school.DataZakonczenia,
				DataLikwidacji = school.DataLikwidacji,
				LiczbaUczniow = school.LiczbaUczniow,
				KategoriaUczniow = school.KategoriaUczniow,
				SpecyfikaSzkoly = school.SpecyfikaSzkoly,
				PodmiotProwadzacyTyp = school.PodmiotProwadzacyTyp,
				PodmiotProwadzacy = school.PodmiotProwadzacyNazwa,
				Geography = MapToGeography(school.Geography),
				AutoUpdate = school.AutoUpdate
			};
		}

		private static Point MapToGeography(GeographyDTO? geography)
		{
			if (geography == null) return null;

			return new Point(new Coordinate { X = geography.X, Y = geography.Y });

		}
	}
}