using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameDB.Migrations
{
    /// <inheritdoc />
    public partial class AddAchievementValuieDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AchievementValue",
                columns: table => new
                {
                    PlayerDbId = table.Column<int>(type: "int", nullable: false),
                    MonsterKill = table.Column<int>(type: "int", nullable: false),
                    ConsumGold = table.Column<int>(type: "int", nullable: false),
                    StageClear = table.Column<int>(type: "int", nullable: false),
                    CurrencyGacha = table.Column<int>(type: "int", nullable: false),
                    BuddySkillUp = table.Column<int>(type: "int", nullable: false),
                    BuddyLevelUp = table.Column<int>(type: "int", nullable: false),
                    HeroSkillUp = table.Column<int>(type: "int", nullable: false),
                    HeroLevelUp = table.Column<int>(type: "int", nullable: false),
                    HeroGacha = table.Column<int>(type: "int", nullable: false),
                    BuddyGacha = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AchievementValue", x => x.PlayerDbId);
                    table.ForeignKey(
                        name: "FK_AchievementValue_Player_PlayerDbId",
                        column: x => x.PlayerDbId,
                        principalTable: "Player",
                        principalColumn: "PlayerDbId",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AchievementValue");
        }
    }
}
