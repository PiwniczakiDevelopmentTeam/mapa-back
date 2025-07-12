namespace mapa_back.Data.RSPOApi
{
	public class SyncResponse
	{
		public List<int> RspoNumber { get; set; } = new();
		public List<string> Exception { get; set; } = new();
	}
}
