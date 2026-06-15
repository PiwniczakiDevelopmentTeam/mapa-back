using mapa_back.Data;

namespace mapa_back.Services
{
	public interface IUsersService
	{
		Task<User?> GetUserByEmailAsync(string email);
		Task<User?> GetUserByIdAsync(int id);

		Task<bool> PostSingleUser(User user);

		Task<List<User>> GetAllUsersAsync();
		Task<bool> UpdateUserAsync(User user);
		Task<bool> DeleteUserAsync(int id);
	}
}