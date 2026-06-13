using mapa_back.Configuration;
using mapa_back.Data.RSPOApi;
using mapa_back.Exceptions;
using mapa_back.Models;
using mapa_back.Models.RSPOApi;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using System;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;

namespace mapa_back.Services
{
    public class RSPOApiService : IRSPOApiService
    {
        private readonly DatabaseContext _dbContext;
        private readonly HttpClient _httpClient;
		private readonly RSPOProgressTracker _progressTracker;
		private readonly RspoApiOptions _config;

		public RSPOApiService(DatabaseContext dbContext, HttpClient httpClient, RSPOProgressTracker progressTracker, IOptions<RspoApiOptions> config)
        {
            _dbContext = dbContext;
            _httpClient = httpClient;
            _progressTracker = progressTracker;
            _config = config.Value;
        }
        

        private int GetNumberOfPages(string responseBody)
        {
            try
            {
                int numberOfPages = 0;
                using (JsonDocument doc = JsonDocument.Parse(responseBody))
                {
                    Regex rx = new Regex("\\d+");
                    JsonElement root = doc.RootElement;
                    var regexMatch = rx.Match(root.GetProperty("hydra:view").GetProperty("hydra:last").ToString());
                    if (regexMatch.Success)
                    {
                        numberOfPages = Int32.Parse(regexMatch.Value);
                    }
                    else
                    {
                        throw new Exception("Couldn't find total pages number");
                    }
                }
                return numberOfPages;
            }
            catch (Exception)
            {
                throw new RSPOToDatabaseException("Unexpected error occurred while trying to get number of pages from response json");
            }
        }
        static List<SchoolApi> GetSchoolsFromResponse(string responseBody)
        {
            try
            {
                using (JsonDocument doc = JsonDocument.Parse(responseBody))
                {
                    List<SchoolApi> schools = JsonSerializer.Deserialize<List<SchoolApi>>(responseBody) ?? new List<SchoolApi>();
                    return schools;
                }
            }
            catch(Exception)
            {
                throw new RSPOToDatabaseException("Unexpected error occurred while trying to GetSchoolsFromResponse JSON");
            }
        }
        private async Task SaveSingleSchoolToDatabase(Point geography, SchoolApi schoolFromApi, List<int> invalidRspoNumbers, List<string> exceptions)
        {
            SchoolFromRSPO? school = new SchoolFromRSPO();

			try
            {
				school = await _dbContext.SchoolsFromRSPO.FirstOrDefaultAsync(element => element.NumerRspo == schoolFromApi.NumerRspo);

                if(school == null)
                {
					school = new SchoolFromRSPO
                    {
                        NumerRspo = schoolFromApi.NumerRspo
                    };
					_dbContext.SchoolsFromRSPO.Add(school);
				}
				school.Geography = geography;
				school.Typ = schoolFromApi.Typ?.Nazwa;
				school.StatusPublicznoPrawny = schoolFromApi.StatusPublicznoPrawny?.Nazwa;
				school.Nazwa = schoolFromApi.Nazwa;
				school.Wojewodztwo = schoolFromApi.Wojewodztwo;
				school.Gmina = schoolFromApi.Gmina;
				school.Powiat = schoolFromApi.Powiat;
				school.Miejscowosc = schoolFromApi.Miejscowosc;
				school.GminaRodzaj = schoolFromApi.GminaRodzaj;
                school.KodPocztowy = schoolFromApi.KodPocztowy;
				school.Ulica = schoolFromApi.Ulica;
				school.NumerBudynku = schoolFromApi.NumerBudynku;
				school.NumerLokalu = schoolFromApi.NumerLokalu;
				school.Email = schoolFromApi.Email;
				school.Telefon = schoolFromApi.Telefon;
				school.StronaInternetowa = schoolFromApi.StronaInternetowa;
				school.DyrektorImie = schoolFromApi.DyrektorImie;
                school.DyrektorNazwisko = schoolFromApi.DyrektorNazwisko;
                school.Nip = schoolFromApi.Nip;
				school.Regon = schoolFromApi.Regon;
				school.DataRozpoczecia = ParseDate(schoolFromApi.DataRozpoczecia);
				school.DataZalozenia = ParseDate(schoolFromApi.DataZalozenia);
				school.DataLikwidacji = ParseDate(schoolFromApi.DataLikwidacji);
                school.DataZakonczenia = ParseDate(schoolFromApi.DataZakonczenia);
				school.LiczbaUczniow = schoolFromApi.LiczbaUczniow;
				school.KategoriaUczniow = schoolFromApi.KategoriaUczniow?.Nazwa;
				school.SpecyfikaSzkoly = schoolFromApi.SpecyfikaSzkoly?.Nazwa;
                school.PodmiotProwadzacy = schoolFromApi.PodmiotProwadzacy?.FirstOrDefault()?.Nazwa;
				school.PodmiotProwadzacyTyp = schoolFromApi.PodmiotProwadzacy?.FirstOrDefault()?.Typ?.Nazwa;
			}
            catch(Exception ex)
            {
                invalidRspoNumbers.Add(school.NumerRspo);
                exceptions.Add(ex.Message);
			}
        }
		private DateOnly? ParseDate(string? dateString)
		{
			if (DateTimeOffset.TryParse(dateString, out var dto))
			{
				return DateOnly.FromDateTime(dto.Date);
			}
			return null;
		}
		private async Task SaveSchoolsToDatabase(List<SchoolApi> schools,List<int> invalidRspoNumbers, List<string> exceptions)
        {
            foreach (var school in schools)
            {
                //Sometimes RSPO APi gives no data about geo. In that case I'll just set 0,0
                Point point = new Point(new Coordinate { X = school.Geo?.Longitude ?? 0, Y = school.Geo?.Latitude ?? 0 });
                await SaveSingleSchoolToDatabase(point, school, invalidRspoNumbers, exceptions);
            }
            try
            {
                await _dbContext.SaveChangesAsync();
            }
            catch(Exception ex)
            {
				throw new RSPOToDatabaseException("Unexpected error occurred while trying to save list of rspo schools data to database");
			}
			GC.Collect();
            GC.WaitForPendingFinalizers();
        }
        public async Task SyncDataFromRSPOApi()
        {
	        if (_progressTracker.IsSyncInProgress)
		        return;

	        _progressTracker.IsSyncInProgress = true;
	        _progressTracker.Exceptions = new List<string>();

	        try
	        {
		        await EnsureSessionAsync();

		        string firstUrl = "api/placowki/?page=1";

		        using var firstResponse = await _httpClient.GetAsync(firstUrl);
		        firstResponse.EnsureSuccessStatusCode();

		        string body = await firstResponse.Content.ReadAsStringAsync();
		        int page = 1;
		        _progressTracker.CurrentPage = page;
				_dbContext.SchoolsFromRSPO.ExecuteDelete();
				while (!string.IsNullOrEmpty(body) && body.Trim() != "[]")
		        {
			        string url = $"api/placowki/?page={page}";
			        using HttpResponseMessage response = await _httpClient.GetAsync(url);
			        response.EnsureSuccessStatusCode();
			        body = await response.Content.ReadAsStringAsync();
			        List<SchoolApi> schools = GetSchoolsFromResponse(body);
			        await SaveSchoolsToDatabase(schools, new(), new());
			        page++;
			        _progressTracker.CurrentPage = page;
				}
	        }
	        catch (Exception ex)
	        {
		        _progressTracker.Exceptions.Add(ex.Message);
	        }
	        finally
	        {
		        _progressTracker.IsSyncInProgress = false;
	        }
        }
		private async Task EnsureSessionAsync()
		{
			await _httpClient.GetAsync("/");
		}

	}
}
