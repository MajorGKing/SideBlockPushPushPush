using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameDB.Migrations
{
    /// <inheritdoc />
    public partial class CurrencyGachaLogDbAdd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CurrencyGachaLog",
                columns: table => new
                {
                    CurrencyGachaLogDbId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PlayerDbId = table.Column<int>(type: "int", nullable: false),
                    Do = table.Column<int>(type: "int", nullable: false),
                    DoMax = table.Column<int>(type: "int", nullable: false),
                    GachaItemResult = table.Column<int>(type: "int", nullable: false),
                    Count = table.Column<int>(type: "int", nullable: false),
                    UnixSeconds = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CurrencyGachaLog", x => x.CurrencyGachaLogDbId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CurrencyGachaLog");
        }
    }
}
