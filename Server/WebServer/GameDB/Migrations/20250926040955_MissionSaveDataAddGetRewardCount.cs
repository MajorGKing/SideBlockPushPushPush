using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameDB.Migrations
{
    /// <inheritdoc />
    public partial class MissionSaveDataAddGetRewardCount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "GetRewardCount",
                table: "MissionSaveData",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GetRewardCount",
                table: "MissionSaveData");
        }
    }
}
