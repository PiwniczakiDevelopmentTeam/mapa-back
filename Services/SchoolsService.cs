using mapa_back.Data;
using mapa_back.Data.DTO;
using mapa_back.Exceptions;
using mapa_back.Mappers;
using mapa_back.Models;
using mapa_back.Models.DTO;
using Microsoft.EntityFrameworkCore;

namespace mapa_back.Services
{
    public class SchoolsService : ISchoolsService
    {
        private readonly DatabaseContext _dbContext;

        public SchoolsService(DatabaseContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<long> GetSchoolsCount()
        {
            try
            {
                long schoolsNumber = await _dbContext.SchoolsActual.CountAsync();
                return schoolsNumber;
            }
            catch (Exception)
            {
                throw new DatabaseException("Unexpected error occurred while trying to count elements in database");
            }
        }
        private bool ValidatePageParametres(int pageNumber, int pageSize)
        {
            if (pageNumber < 1 || pageSize < 1)
            {
                return false;
            }
            return true;
        }
        public async Task<PagedResult<SchoolActual>> GetSchoolsPage(int size, int pageNumber, List<FilterParams>? filters = null)
        {
            if (!ValidatePageParametres(pageNumber, size))
            {
                throw new ArgumentException("Parametres not valid");
            }
			try
			{
				IQueryable<SchoolActual> query = _dbContext.SchoolsActual;
				if (filters != null)
				{
					query = FilterBuilder.ApplyFilters(query, filters);
				}
				int totalCount = await query.CountAsync();
				List<SchoolActual> schoolsPage = await query.Where(p => true).Skip((pageNumber - 1) * size).Take(size).ToListAsync();

				PagedResult<SchoolActual> result = new PagedResult<SchoolActual>
				{
					Items = schoolsPage,
					TotalCount = totalCount
				};

				return result;
			}
			catch (SchoolServiceException)
            {
                throw;
            }
            catch(Exception)
            {
                throw new Exception("Unexpected error occurred while trying to get school page from database");
            }
        }
        public async Task DeleteSingleSchool(int rspoId)
        {
            try
            {
                SchoolActual school = await _dbContext.SchoolsActual.FirstOrDefaultAsync(p => p.NumerRspo == rspoId) ?? throw new DatabaseException($"Couldnt find school with given Id: {rspoId}");
                _dbContext.SchoolsActual.Remove(school);
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception)
            {
                throw new DatabaseException($"Unexpected eror occurred while trying to delete school with given Id: {rspoId} from database");
            }
		}
		//Auto update from RSPO
		public async Task<bool> SyncRspoToActual()
		{
			const int size = 1000;
			int pageNumber = 0;

			while (true)
			{
				var schoolsFromRSPO = await _dbContext.SchoolsFromRSPO
					.OrderBy(x => x.Id)
					.Skip(pageNumber * size)
					.Take(size)
					.ToListAsync();

				if (!schoolsFromRSPO.Any())
					break;

				var rspoIds = schoolsFromRSPO.Select(x => x.NumerRspo).ToList();

				var actualSchools = await _dbContext.SchoolsActual
					.Where(x => rspoIds.Contains(x.NumerRspo))
					.ToListAsync();

				var actualDict = actualSchools
					.ToDictionary(x => x.NumerRspo);

				var toInsert = new List<SchoolActual>();

				foreach (var rspo in schoolsFromRSPO)
				{
					if (actualDict.TryGetValue(rspo.NumerRspo, out var actual))
					{
						if (!actual.AutoUpdate)
							continue;

						actual.UpdateFrom(rspo);
					}
					else
					{
						var newSchool = new SchoolActual(rspo)
						{
							AutoUpdate = true
						};

						toInsert.Add(newSchool);
					}
				}

				if (toInsert.Any())
					await _dbContext.SchoolsActual.AddRangeAsync(toInsert);

				await _dbContext.SaveChangesAsync();

				pageNumber++;
			}

			return true;
		}
		public async Task DeleteManySchools(List<int> rspoIds)
        {
            try
            {
                List<SchoolActual> schools = await _dbContext.SchoolsActual.Where(school => rspoIds.Contains(school.NumerRspo)).ToListAsync();
                if (!schools.Any())
                {
                    throw new DatabaseException("No schools found with the provided IDs.");
                }
                _dbContext.SchoolsActual.RemoveRange(schools);
                await _dbContext.SaveChangesAsync();
            }
            catch(Exception)
            {
                throw new DatabaseException("An unexpected error occurred while deleting schools from the database.");
            }

		}
       
        public async Task<ChangedSchoolsResponse> GetChangedSchoolsList(int size, int pageNumber)
        {
            try
            {
                ChangedSchoolsResponse response = new ChangedSchoolsResponse();
                List<School> currentSchools = _dbContext.SchoolsActual.ToList<School>();
                List<School> archivedSchools = _dbContext.SchoolsFromRSPO.ToList<School>();

                List<School> differentNewSchools = currentSchools.Except(archivedSchools).ToList();
                List<School> differentArchivedSchools = archivedSchools.Except(currentSchools).ToList();

				List<School> notExistingSchools = differentNewSchools.Where(x => !differentArchivedSchools.Any(archived => archived.NumerRspo == x.NumerRspo)).ToList();
				List<School> newSchools = differentArchivedSchools.Where(x => !differentNewSchools.Any(newSchool => newSchool.NumerRspo == x.NumerRspo)).ToList();

                List<School> differentSchools = differentNewSchools.Where(x => differentArchivedSchools.Any(archived => archived.NumerRspo == x.NumerRspo)).ToList();
				Dictionary<int, School> archivedDict = archivedSchools.ToDictionary(s => s.NumerRspo);


                foreach (School current in differentSchools)
                {
                    if (archivedDict.TryGetValue(current.NumerRspo, out var archived))
                    {
                        response.ChangedSchools.Add(new ChangedSchool(archived, current));
                    }
                }
                response.NewSchools = newSchools;
                response.NotExistingSchools = notExistingSchools;
                response.SchoolsCount = Math.Max(currentSchools.Count, archivedSchools.Count);
                response.ChangedSchools.Skip(pageNumber - 1).Take(size).ToList();
				return response;
			}
            catch (Exception)
            {
                throw new DatabaseException("An unexpected error occurred while trying to get data from database");
            }
            
        }
		public async Task<SchoolDTO> GetSingleSchool(int rspoId)
        {
			if (rspoId <= 0)
			{
				throw new ArgumentException("Id has to be higher than 0");
			}

			School? singleSchool = await _dbContext.SchoolsActual.FirstOrDefaultAsync(s => s.NumerRspo == rspoId);
            if (singleSchool == null) return null;
            return SchoolMapper.MapToDTO(singleSchool);
		}

		public async Task<SchoolDTO> GetSingleSchoolFromRSPO(int rspoId)
		{
			if (rspoId <= 0)
			{
				throw new ArgumentException("Id has to be higher than 0");
			}

			School? singleSchool = await _dbContext.SchoolsFromRSPO.FirstOrDefaultAsync(s => s.NumerRspo == rspoId);
			if (singleSchool == null) return null;
			return SchoolMapper.MapToDTO(singleSchool);
		}
		public async Task<ChangedSchool> GetSingleChangedSchool(int rspoId)
        {
            School? singleSchool = _dbContext.SchoolsActual.FirstOrDefault(s => s.NumerRspo == rspoId);
            if(singleSchool == null)
            {
                throw new SchoolServiceException("Couldnt find school with given Id in database");
            }
			School? singleSchoolFromRSPO = _dbContext.SchoolsFromRSPO.FirstOrDefault(s => s.NumerRspo == singleSchool.NumerRspo);
            if(singleSchoolFromRSPO == null)
            {
                throw new SchoolServiceException($"Couldn't find matching school in RSPO Database with given rspo number: {singleSchool.NumerRspo}");
            }

            ChangedSchool changedSchool = new ChangedSchool(singleSchool,singleSchoolFromRSPO);
            return changedSchool;

		}

        public async Task<bool> PostSingleSchool(SchoolActual school)
        {
            if(school == null)
            {
                throw new SchoolServiceException("School cannot be null");
            }
            try
            {
                _dbContext.SchoolsActual.Add(new SchoolActual(school));
                await _dbContext.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                throw new DatabaseException("Couldn't add given school to database");
            }
        }

		public async Task<bool> PostManySchools(List<SchoolActual> schools)
		{
			if (schools == null)
			{
				throw new SchoolServiceException("School cannot be null");
			}
            if(schools.Count <= 0)
            {
                throw new SchoolServiceException("Schools cannot be empty list");
            }
			try
			{
                foreach(var school in schools)
                {
					_dbContext.SchoolsActual.Add(new SchoolActual(school));
				}
                await _dbContext.SaveChangesAsync();
				return true;
			}
			catch (Exception ex)
			{
				throw new DatabaseException("Couldn't add given school to database");
			}
		}

		public async Task<bool> UpdateSingleSchool(SchoolActual school)
		{
			if (school == null)
			{
				throw new SchoolServiceException("School cannot be null");
			}
			if (school.Id <= 0)
			{
				throw new ArgumentException("Id has to be higher than 0");
			}
			try
			{
                if (!_dbContext.SchoolsActual.Any(s => s.Id == school.Id))
                {
                    throw new SchoolServiceException("Couldn't find school with matching Id in database");
                }
                School editetSchool = await _dbContext.SchoolsActual.FirstOrDefaultAsync(x => x.Id == school.Id);
                _dbContext.Entry(editetSchool).CurrentValues.SetValues(school);
				await _dbContext.SaveChangesAsync();
				return true;
			}
			catch (Exception ex)
			{
				throw new DatabaseException("Couldn't update given school in database");
			}
		}

		public async Task<bool> UpdateManySchools(List<SchoolActual> schools)
		{
			if (schools == null)
			{
				throw new SchoolServiceException("School cannot be null");
			}
			if (schools.Count <= 0)
			{
				throw new SchoolServiceException("Schools cannot be empty list");
			}
			try
			{
				var schoolIds = schools.Select(s => s.Id).ToList();

				var existingSchools = await _dbContext.SchoolsActual
					.Where(s => schoolIds.Contains(s.Id))
					.ToListAsync();

				if (existingSchools.Count != schools.Count)
				{
					throw new ArgumentException("Some schools with given id do not exist in the database");
				}
				foreach (var school in schools)
                {
					 School editetSchool = await _dbContext.SchoolsActual.FirstOrDefaultAsync(x => x.Id == school.Id);
					_dbContext.Entry(editetSchool).CurrentValues.SetValues(school);
				}
				await _dbContext.SaveChangesAsync();
				return true;
			}
			catch (Exception ex)
			{
				throw new DatabaseException("Couldn't update given school in database");
			}
		}

        public async Task<List<SchoolDTO>> GetMissingSchoolsInRSPOTable(int size, int pageNumber)
        {
            try
            {
                var missingSchools = await _dbContext.SchoolsActual.Where(school => !_dbContext.SchoolsFromRSPO.Any(rspo => rspo.NumerRspo == school.NumerRspo))
                    .Select(x => SchoolMapper.MapToDTO(x)).Skip(size * (pageNumber-1)).Take(size).ToListAsync();
				return missingSchools;
			}
            catch(Exception)
            {
                throw new SchoolServiceException("Unexpected error occurred while trying to get missing schools from school table");

			}
        }

        public async Task<int> GetMissingSchoolsInRSPOTableCount()
        {
			try
			{
				var missingSchoolsCount = await _dbContext.SchoolsActual
                    .CountAsync(school => !_dbContext.SchoolsFromRSPO
                    .Any(rspo => rspo.NumerRspo == school.NumerRspo));

				return missingSchoolsCount;
			}
			catch (Exception)
			{
				throw new SchoolServiceException("Unexpected error occurred while trying to get missing schools from school table");

			}
		}

		public async Task<List<SchoolDTO>> GetMissingSchoolsInSchoolsTable(int size, int pageNumber)
		{
			try
			{
				var missingSchools = await _dbContext.SchoolsFromRSPO.Where(school => !_dbContext.SchoolsActual.Any(rspo => rspo.NumerRspo == school.NumerRspo))
					.Select(x => SchoolMapper.MapToDTO(x)).Skip(size * (pageNumber-1)).Take(size).ToListAsync();
				return missingSchools;
			}
			catch (Exception)
			{
				throw new SchoolServiceException("Unexpected error occurred while trying to get missing schools from school table");

			}
		}

		public async Task<int> GetMissingSchoolsInSchoolsTableCount()
		{
			try
			{
				var missingSchoolsCount = await _dbContext.SchoolsFromRSPO
					.CountAsync(school => !_dbContext.SchoolsActual
					.Any(rspo => rspo.NumerRspo == school.NumerRspo));

				return missingSchoolsCount;
			}
			catch (Exception)
			{
				throw new SchoolServiceException("Unexpected error occurred while trying to get missing schools from school table");

			}
		}
		
        public async Task AddSchoolsFromRSPOTableToMapSchoolTable()
        {
            int skip = 0;
            int limit = 100;
            long count = _dbContext.SchoolsFromRSPO.Count();
            List<School> schools = _dbContext.SchoolsFromRSPO.Skip(skip).Take(limit).ToList<School>();
            while(schools.Count > 0)
            {
				List<SchoolActual> actualSchools = schools
					.Select(s => new SchoolActual(s))
					.ToList(); 

                await _dbContext.SchoolsActual.AddRangeAsync(actualSchools);
				await _dbContext.SaveChangesAsync();
                skip += 100;
				schools = _dbContext.SchoolsFromRSPO.Skip(skip).Take(limit).ToList<School>();
                Console.WriteLine($"Readed {skip}/{count} schools");
			}

        }

	}
}
