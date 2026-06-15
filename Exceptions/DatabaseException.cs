namespace mapa_back.Exceptions
{
	public class DatabaseException : Exception
	{
		public DatabaseException(string message) : base(message) { }
	}
}