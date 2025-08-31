using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GameDB
{
	public class GameDbContext : DbContext
	{
		public DbSet<PlayerDb> PlayerDb { get; set; }

		static readonly ILoggerFactory _logger = LoggerFactory.Create(builder => { builder.AddConsole(); });
		public static string ConnectionString = "Data Source=localhost,1433;Initial Catalog=GameDB;User ID=sa;Password=YourPassword123;Encrypt=False;Trust Server Certificate=True";



		public GameDbContext()
		{

			if (Environment.OSVersion.Platform == PlatformID.Win32NT)
			{
				// Windows에서 LocalDB 사용
				ConnectionString = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=GameDB;Integrated Security=True;Connect Timeout=30;Encrypt=False;Trust Server Certificate=False;Application Intent=ReadWrite;Multi Subnet Failover=False";
			}
			else
			{
				// Mac에서 Docker로 실행 중인 SQL Server Express 연결
				ConnectionString = "Data Source=localhost,1433;Initial Catalog=GameDB;User ID=sa;Password=YourPassword123;Encrypt=False;Trust Server Certificate=True";
			}

		}

		protected override void OnConfiguring(DbContextOptionsBuilder options)
		{
			options
				.UseLoggerFactory(_logger)
				.UseSqlServer(ConnectionString);
		}

		protected override void OnModelCreating(ModelBuilder builder)
		{
			builder.Entity<PlayerDb>()
				.HasIndex(t => t.PlayerDbId)
				.IsUnique();
		}
	}
}
