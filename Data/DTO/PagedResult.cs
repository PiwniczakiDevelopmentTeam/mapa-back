namespace mapa_back.Data.DTO
{
	public class PagedResult<T>
	{
		public List<T> Items { get; set; }
		public int TotalCount { get; set; }
	}
}
