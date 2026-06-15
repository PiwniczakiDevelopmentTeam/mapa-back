using mapa_back.Data;
using mapa_back.Models;
using mapa_back.Models.DTO;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations.Schema;

namespace mapa_back
{
	public class DatabaseContext : DbContext
	{
		public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options) { }

		public DbSet<SchoolFromRSPO> SchoolsFromRSPO { get; set; }
		public DbSet<SchoolActual> SchoolsActual { get; set; }

		public DbSet<User> Users { get; set; }
	}
}