using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameDB.Migrations
{
    /// <inheritdoc />
    public partial class BuddySkillListAdd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SkillTemplateIdString",
                table: "BuddySaveData",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SkillTemplateIdString",
                table: "BuddySaveData");
        }
    }
}
