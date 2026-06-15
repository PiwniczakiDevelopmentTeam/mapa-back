using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace mapa_back.Data
{
	[Table("users")]
	public class User
	{
		[Required]
		[Key]
		[Column("id")]
		public int Id { get; set; }

		[Required]
		[Column("email")]
		public required string Email { get; set; }

		[Required]
		[Column("password")]
		public required string Password { get; set; }

		[Required]
		[Column("firstname")]
		public required string FirstName { get; set; }

		[Required]
		[Column("lastname")]

		public required string LastName { get; set; }

		[Required]
		[Column("id_role")]

		public required int IdRole { get; set; }
	}
}