using mapa_back.Data;
using mapa_back.Data.DTO;
using mapa_back.Models;
using mapa_back.Models.DTO;

namespace mapa_back.Services
{
    public interface ISchoolsService
    {
        Task<PagedResult<SchoolActual>> GetSchoolsPage(int size, int pageNumber, List<FilterParams>? filters = null);
		Task<SchoolDTO> GetSingleSchool(int rspoId);
		Task<SchoolDTO> GetSingleSchoolFromRSPO(int rspoId);
		Task DeleteSingleSchool(int rspoId);
        Task DeleteManySchools(List<int> rspoIds);
        Task<ChangedSchoolsResponse> GetChangedSchoolsList(int size, int pageNumber);
        Task<long> GetSchoolsCount();
        Task<ChangedSchool> GetSingleChangedSchool(int rspoId);
        Task<bool> PostSingleSchool(SchoolActual school);
        Task<bool> PostManySchools(List<SchoolActual> schools);
        Task<bool> UpdateSingleSchool(SchoolActual school);
		Task<bool> UpdateManySchools(List<SchoolActual> schools);
        Task<bool> SyncRspoToActual();
        Task<List<SchoolDTO>> GetMissingSchoolsInRSPOTable(int size, int pageNumber);
		Task<List<SchoolDTO>> GetMissingSchoolsInSchoolsTable(int size, int pageNumber);
        Task<int> GetMissingSchoolsInRSPOTableCount();
		Task<int> GetMissingSchoolsInSchoolsTableCount();
        Task<SchoolActual> GetSchoolForMap(int id);
        Task<List<SchoolActual>> GetSchoolsForMap();
	}
}
