using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameDB.Migrations
{
    /// <inheritdoc />
    public partial class BuddySelectedIntChange : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsSelected",
                table: "BuddySaveData");

            migrationBuilder.AddColumn<int>(
                name: "SelectedNumber",
                table: "BuddySaveData",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SelectedNumber",
                table: "BuddySaveData");

            migrationBuilder.AddColumn<bool>(
                name: "IsSelected",
                table: "BuddySaveData",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
