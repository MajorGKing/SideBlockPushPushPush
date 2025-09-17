using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameDB.Migrations
{
    /// <inheritdoc />
    public partial class AddHBuddyGachaLogTableAndHeroGachaLogChanged : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "GachaItemResult",
                table: "HeroGachaLog",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<int>(
                name: "Count",
                table: "HeroGachaLog",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "BuddyGachaLog",
                columns: table => new
                {
                    BuddyGachaLogDbId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PlayerDbId = table.Column<int>(type: "int", nullable: false),
                    Do = table.Column<int>(type: "int", nullable: false),
                    DoMax = table.Column<int>(type: "int", nullable: false),
                    BuddyTemplateId = table.Column<int>(type: "int", nullable: false),
                    Rarity = table.Column<int>(type: "int", nullable: false),
                    IsDuplicate = table.Column<bool>(type: "bit", nullable: false),
                    DuplicateRewardType = table.Column<int>(type: "int", nullable: true),
                    DuplicateRewardCount = table.Column<int>(type: "int", nullable: false),
                    UnixSeconds = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BuddyGachaLog", x => x.BuddyGachaLogDbId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BuddyGachaLog");

            migrationBuilder.DropColumn(
                name: "Count",
                table: "HeroGachaLog");

            migrationBuilder.AlterColumn<string>(
                name: "GachaItemResult",
                table: "HeroGachaLog",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");
        }
    }
}
