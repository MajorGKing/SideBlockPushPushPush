using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;

namespace GameDB
{
	public class GameDbContext : DbContext
	{
		public DbSet<TestDb> Tests { get; set; }

		public GameDbContext()
		{
		}

		static readonly ILoggerFactory _logger = LoggerFactory.Create(builder => { builder.AddConsole(); });
		public static string ConnectionString = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=GameDB;Integrated Security=True;Connect Timeout=30;Encrypt=False;Trust Server Certificate=False;Application Intent=ReadWrite;Multi Subnet Failover=False";

		protected override void OnConfiguring(DbContextOptionsBuilder options)
		{
			options
				.UseLoggerFactory(_logger)
				.UseSqlServer(ConnectionString);
		}

		protected override void OnModelCreating(ModelBuilder builder)
		{
			builder.Entity<TestDb>()
				.HasIndex(t => t.Name)
				.IsUnique();
		}
	}
}
 