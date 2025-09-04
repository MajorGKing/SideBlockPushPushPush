using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameDB.Migrations
{
    /// <inheritdoc />
    public partial class init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Player",
                columns: table => new
                {
                    PlayerDbId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UniqueId = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    UserLevel = table.Column<int>(type: "int", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Stamina = table.Column<int>(type: "int", nullable: false),
                    BGMOn = table.Column<bool>(type: "bit", nullable: false),
                    EffectSoundOn = table.Column<bool>(type: "bit", nullable: false),
                    LastMissionTime = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Player", x => x.PlayerDbId);
                });

            migrationBuilder.CreateTable(
                name: "AchievementClearList",
                columns: table => new
                {
                    AchievementClearListDbId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TemplateId = table.Column<int>(type: "int", nullable: false),
                    PlayerDbId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AchievementClearList", x => x.AchievementClearListDbId);
                    table.ForeignKey(
                        name: "FK_AchievementClearList_Player_PlayerDbId",
                        column: x => x.PlayerDbId,
                        principalTable: "Player",
                        principalColumn: "PlayerDbId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AchievementSaveData",
                columns: table => new
                {
                    AchievementSaveDataDbId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TemplateId = table.Column<int>(type: "int", nullable: false),
                    StackedPoint = table.Column<int>(type: "int", nullable: false),
                    MissionState = table.Column<int>(type: "int", nullable: false),
                    OriginalTemplateId = table.Column<int>(type: "int", nullable: false),
                    IsCleared = table.Column<bool>(type: "bit", nullable: false),
                    PlayerDbId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AchievementSaveData", x => x.AchievementSaveDataDbId);
                    table.ForeignKey(
                        name: "FK_AchievementSaveData_Player_PlayerDbId",
                        column: x => x.PlayerDbId,
                        principalTable: "Player",
                        principalColumn: "PlayerDbId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BuddySaveData",
                columns: table => new
                {
                    BuddySaveDataDbId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TemplateId = table.Column<int>(type: "int", nullable: false),
                    IsSelected = table.Column<bool>(type: "bit", nullable: false),
                    PlayerDbId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BuddySaveData", x => x.BuddySaveDataDbId);
                    table.ForeignKey(
                        name: "FK_BuddySaveData_Player_PlayerDbId",
                        column: x => x.PlayerDbId,
                        principalTable: "Player",
                        principalColumn: "PlayerDbId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Currency",
                columns: table => new
                {
                    PlayerDbId = table.Column<int>(type: "int", nullable: false),
                    Gold = table.Column<int>(type: "int", nullable: false),
                    Dia = table.Column<int>(type: "int", nullable: false),
                    BlueGem = table.Column<int>(type: "int", nullable: false),
                    GreenGem = table.Column<int>(type: "int", nullable: false),
                    YellowGem = table.Column<int>(type: "int", nullable: false),
                    StoneArmor = table.Column<int>(type: "int", nullable: false),
                    StoneBelt = table.Column<int>(type: "int", nullable: false),
                    StoneBoots = table.Column<int>(type: "int", nullable: false),
                    StoneGloves = table.Column<int>(type: "int", nullable: false),
                    StoneRing = table.Column<int>(type: "int", nullable: false),
                    StoneWeapon = table.Column<int>(type: "int", nullable: false),
                    Exp = table.Column<int>(type: "int", nullable: false),
                    ScrollArmor = table.Column<int>(type: "int", nullable: false),
                    ScrollBelt = table.Column<int>(type: "int", nullable: false),
                    ScrollBoots = table.Column<int>(type: "int", nullable: false),
                    ScrollGloves = table.Column<int>(type: "int", nullable: false),
                    ScrollRing = table.Column<int>(type: "int", nullable: false),
                    ScrollWeapon = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Currency", x => x.PlayerDbId);
                    table.ForeignKey(
                        name: "FK_Currency_Player_PlayerDbId",
                        column: x => x.PlayerDbId,
                        principalTable: "Player",
                        principalColumn: "PlayerDbId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HeroSaveData",
                columns: table => new
                {
                    HeroSaveDataDbId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TemplateId = table.Column<int>(type: "int", nullable: false),
                    IsSelected = table.Column<bool>(type: "bit", nullable: false),
                    NowExp = table.Column<int>(type: "int", nullable: false),
                    MaxExp = table.Column<int>(type: "int", nullable: false),
                    PlayerDbId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HeroSaveData", x => x.HeroSaveDataDbId);
                    table.ForeignKey(
                        name: "FK_HeroSaveData_Player_PlayerDbId",
                        column: x => x.PlayerDbId,
                        principalTable: "Player",
                        principalColumn: "PlayerDbId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MissionSaveData",
                columns: table => new
                {
                    MissionSaveDataDbId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TemplateId = table.Column<int>(type: "int", nullable: false),
                    StackedPoint = table.Column<int>(type: "int", nullable: false),
                    MissionState = table.Column<int>(type: "int", nullable: false),
                    PlayerDbId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MissionSaveData", x => x.MissionSaveDataDbId);
                    table.ForeignKey(
                        name: "FK_MissionSaveData_Player_PlayerDbId",
                        column: x => x.PlayerDbId,
                        principalTable: "Player",
                        principalColumn: "PlayerDbId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StageClear",
                columns: table => new
                {
                    StageClearDbId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TemplateId = table.Column<int>(type: "int", nullable: false),
                    isEnable = table.Column<bool>(type: "bit", nullable: false),
                    isClear = table.Column<bool>(type: "bit", nullable: false),
                    PlayerDbId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StageClear", x => x.StageClearDbId);
                    table.ForeignKey(
                        name: "FK_StageClear_Player_PlayerDbId",
                        column: x => x.PlayerDbId,
                        principalTable: "Player",
                        principalColumn: "PlayerDbId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AchievementClearList_PlayerDbId",
                table: "AchievementClearList",
                column: "PlayerDbId");

            migrationBuilder.CreateIndex(
                name: "IX_AchievementSaveData_PlayerDbId",
                table: "AchievementSaveData",
                column: "PlayerDbId");

            migrationBuilder.CreateIndex(
                name: "IX_BuddySaveData_PlayerDbId",
                table: "BuddySaveData",
                column: "PlayerDbId");

            migrationBuilder.CreateIndex(
                name: "IX_HeroSaveData_PlayerDbId",
                table: "HeroSaveData",
                column: "PlayerDbId");

            migrationBuilder.CreateIndex(
                name: "IX_MissionSaveData_PlayerDbId",
                table: "MissionSaveData",
                column: "PlayerDbId");

            migrationBuilder.CreateIndex(
                name: "IX_Player_UniqueId",
                table: "Player",
                column: "UniqueId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StageClear_PlayerDbId",
                table: "StageClear",
                column: "PlayerDbId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AchievementClearList");

            migrationBuilder.DropTable(
                name: "AchievementSaveData");

            migrationBuilder.DropTable(
                name: "BuddySaveData");

            migrationBuilder.DropTable(
                name: "Currency");

            migrationBuilder.DropTable(
                name: "HeroSaveData");

            migrationBuilder.DropTable(
                name: "MissionSaveData");

            migrationBuilder.DropTable(
                name: "StageClear");

            migrationBuilder.DropTable(
                name: "Player");
        }
    }
}
