using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameDB.Migrations
{
    /// <inheritdoc />
    public partial class AddKillListInHero : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SkillTemplateIdString",
                table: "HeroSaveData",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SkillTemplateIdString",
                table: "HeroSaveData");
        }
    }
}
