using AccountDB;
using AccountServer.Services;
using Microsoft.EntityFrameworkCore;
using GameDB;
using Server.Data;

namespace AccountServer
{
	public class Program
	{
		public static void Main(string[] args)
		{
            ConfigManager.LoadConfig();
            DataManager.LoadData();

            var builder = WebApplication.CreateBuilder(args);

			// Add services to the container.

			builder.Services.AddControllers();
			// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
			builder.Services.AddEndpointsApiExplorer();
			//builder.Services.AddSwaggerGen();

            // GameDbContext를 등록합니다.
            var connectionString = builder.Configuration.GetConnectionString("GameDBConnection");
            builder.Services.AddDbContext<GameDbContext>(options =>
            {
                options.UseSqlServer(connectionString);
            });

            builder.Services.AddDbContext<AccountDbContext>();

            builder.Services.AddSingleton<FacebookService>();
            builder.Services.AddSingleton<JwtTokenService>();
            builder.Services.AddScoped<PlayerService>();
            builder.Services.AddScoped<AccountService>();
            builder.Services.AddScoped<CurrencyService>();
            builder.Services.AddScoped<HeroService>();
            builder.Services.AddScoped<BuddyService>();
            builder.Services.AddScoped<ShopService>();

            var app = builder.Build();

			// Configure the HTTP request pipeline.
			//if (app.Environment.IsDevelopment())
			//{
			//	app.UseSwagger();
			//	app.UseSwaggerUI();
			//}

			app.UseHttpsRedirection();

			app.UseAuthorization();


			app.MapControllers();

			app.Run();
		}
	}
}
