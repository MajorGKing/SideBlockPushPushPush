using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace AccountDB
{
	public class AccountDbContext : DbContext
	{
		public DbSet<AccountDb> Accounts { get; set; }

		static readonly ILoggerFactory _logger = LoggerFactory.Create(builder => { builder.AddConsole(); });

		//public static string ConnectionString = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=AccountDB;Integrated Security=True;Connect Timeout=30;Encrypt=False;Trust Server Certificate=False;Application Intent=ReadWrite;Multi Subnet Failover=False";

		public AccountDbContext()
		{
		}

		protected override void OnConfiguring(DbContextOptionsBuilder options)
		{

			string ConnectionString;
			// Conditionally set the connection string based on the operating system.
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                ConnectionString = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=AccountDB;Integrated Security=True;Connect Timeout=30;Encrypt=False;Trust Server Certificate=False;Application Intent=ReadWrite;Multi Subnet Failover=False";
            }
            else // Assume macOS/Linux if not Windows
            {
                // Docker container connection string for macOS/Linux
                ConnectionString = @"Server=localhost,1433;Database=AccountDB;User Id=sa;Password=YourStrongPassword1@#;Encrypt=False;TrustServerCertificate=True";
            }

			options
				.UseLoggerFactory(_logger)
				.UseSqlServer(ConnectionString);
		}

		protected override void OnModelCreating(ModelBuilder builder)
		{
			builder.Entity<AccountDb>()
				.HasIndex(t => t.LoginProviderUserId)
				.IsUnique();
		}
	}
}
