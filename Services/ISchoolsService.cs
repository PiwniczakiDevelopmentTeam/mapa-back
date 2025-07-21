using mapa_back.Data;
using mapa_back.Data.DTO;
using mapa_back.Models;
using mapa_back.Models.DTO;

namespace mapa_back.Services
{
    public interface ISchoolsService
    {
        Task<PagedResult<School>> GetSchoolsPage(int size, int pageNumber, List<FilterParams>? filters = null);
		Task<SchoolDTO> GetSingleSchool(int id);
		Task DeleteSingleSchool(int id);
        Task DeleteManySchools(List<int> ids);
        Task<ChangedSchoolsResponse> GetChangedSchoolsList(int size, int pageNumber);
        Task<long> GetSchoolsCount();
        Task<ChangedSchool> GetSingleChangedSchool(int id);
        Task<bool> PostSingleSchool(School school);
        Task<bool> PostManySchools(List<School> schools);
        Task<bool> UpdateSingleSchool(School school);
		Task<bool> UpdateManySchools(List<School> schools);
        Task<bool> AddSchoolsFromRSPOTableToActualSchoolTable();
        Task<List<SchoolDTO>> GetMissingSchoolsInRSPOTable(int size, int pageNumber);
		Task<List<SchoolDTO>> GetMissingSchoolsInSchoolsTable(int size, int pageNumber);
        Task<int> GetMissingSchoolsInRSPOTableCount();
		Task<int> GetMissingSchoolsInSchoolsTableCount();
        Task AddSchoolsFromRSPOTableToMapSchoolTable();



	}
}
