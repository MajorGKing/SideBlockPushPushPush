using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameDB.Migrations
{
    /// <inheritdoc />
    public partial class AchievementSaveDataDbRemoveOriginalTempalteId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OriginalTemplateId",
                table: "AchievementSaveData");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "OriginalTemplateId",
                table: "AchievementSaveData",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
