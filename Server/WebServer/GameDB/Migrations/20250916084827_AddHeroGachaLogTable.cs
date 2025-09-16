using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameDB.Migrations
{
    /// <inheritdoc />
    public partial class AddHeroGachaLogTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "HeroGachaLog",
                columns: table => new
                {
                    HeroGachaLogDbId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PlayerDbId = table.Column<int>(type: "int", nullable: false),
                    Do = table.Column<int>(type: "int", nullable: false),
                    DoMax = table.Column<int>(type: "int", nullable: false),
                    GachaItemResult = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UnixSeconds = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HeroGachaLog", x => x.HeroGachaLogDbId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HeroGachaLog");
        }
    }
}
