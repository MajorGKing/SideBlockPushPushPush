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
        public DbSet<PlayerDb> Players { get; set; }
        public DbSet<CurrencyDb> Currencies { get; set; }
        public DbSet<HeroSaveDataDb> Heroes { get; set; }
        public DbSet<BuddySaveDataDb> Buddies { get; set; }
        public DbSet<StageClearDb> StageClears { get; set; }
        public DbSet<MissionSaveDataDb> Missions { get; set; }
        public DbSet<AchievementSaveDataDb> Achievements { get; set; }
        public DbSet<AchievementClearListDb> AchievementClearLists { get; set; }


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
            // PlayerDb의 UniqueId에 고유 인덱스 설정
            builder.Entity<PlayerDb>()
                .HasIndex(p => p.UniqueId)
                .IsUnique();

            // PlayerDb와 CurrencyDb의 1:1 관계 설정
            builder.Entity<PlayerDb>()
                .HasOne(p => p.Currency)
                .WithOne(c => c.Player)
                .HasForeignKey<CurrencyDb>(c => c.PlayerDbId);

            // PlayerDb와 HeroSaveDataDb의 1:N 관계 설정
            builder.Entity<PlayerDb>()
                .HasMany(p => p.Heroes)
                .WithOne(h => h.Player)
                .HasForeignKey(h => h.PlayerDbId);

            // PlayerDb와 BuddySaveDataDb의 1:N 관계 설정
            builder.Entity<PlayerDb>()
                .HasMany(p => p.Buddies)
                .WithOne(b => b.Player)
                .HasForeignKey(b => b.PlayerDbId);

            // PlayerDb와 StageClearDb의 1:N 관계 설정
            builder.Entity<PlayerDb>()
                .HasMany(p => p.StageClears)
                .WithOne(s => s.Player)
                .HasForeignKey(s => s.PlayerDbId);

            // PlayerDb와 MissionSaveDataDb의 1:N 관계 설정
            builder.Entity<PlayerDb>()
                .HasMany(p => p.Missions)
                .WithOne(m => m.Player)
                .HasForeignKey(m => m.PlayerDbId);

            // PlayerDb와 AchievementSaveDataDb의 1:N 관계 설정
            builder.Entity<PlayerDb>()
                .HasMany(p => p.Achievements)
                .WithOne(a => a.Player)
                .HasForeignKey(a => a.PlayerDbId);

            // PlayerDb와 AchievementClearListDb의 1:N 관계 설정
            builder.Entity<PlayerDb>()
                .HasMany(p => p.AchievementClearList)
                .WithOne(ac => ac.Player)
                .HasForeignKey(ac => ac.PlayerDbId);
        }
    }
}
 