using mapa_back.Data.RSPOApi;

namespace mapa_back.Services
{
    public interface IRSPOApiService
    {
        Task<SyncResponse> SyncDataFromRSPOApi();
    }
}
