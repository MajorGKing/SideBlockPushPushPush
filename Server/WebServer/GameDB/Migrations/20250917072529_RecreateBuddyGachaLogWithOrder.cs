using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameDB.Migrations
{
    /// <inheritdoc />
    public partial class RecreateBuddyGachaLogWithOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 0. Drop PK before renaming, otherwise constraint name conflict
            migrationBuilder.DropPrimaryKey(
                name: "PK_BuddyGachaLog",
                table: "BuddyGachaLog");

            // 1. Rename the old table
            migrationBuilder.RenameTable(
                name: "BuddyGachaLog",
                newName: "BuddyGachaLog_Old");

            // 2. Re-add PK on old table so it’s valid
            migrationBuilder.AddPrimaryKey(
                name: "PK_BuddyGachaLog_Old",
                table: "BuddyGachaLog_Old",
                column: "BuddyGachaLogDbId");

            // 3. Create new table with correct column order
            migrationBuilder.CreateTable(
                name: "BuddyGachaLog",
                columns: table => new
                {
                    BuddyGachaLogDbId = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PlayerDbId = table.Column<int>(nullable: false),
                    Do = table.Column<int>(nullable: false),
                    DoMax = table.Column<int>(nullable: false),
                    BuddyTemplateId = table.Column<int>(nullable: false),
                    BuddyGachaName = table.Column<string>(nullable: false, defaultValue: ""),
                    Rarity = table.Column<int>(nullable: false),
                    IsDuplicate = table.Column<bool>(nullable: false),
                    DuplicateRewardType = table.Column<int>(nullable: true),
                    DuplicateRewardCount = table.Column<int>(nullable: false),
                    UnixSeconds = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BuddyGachaLog", x => x.BuddyGachaLogDbId);
                });

            // 4. Copy old data into new table
            migrationBuilder.Sql(@"
    SET IDENTITY_INSERT BuddyGachaLog ON;

    INSERT INTO BuddyGachaLog (
        BuddyGachaLogDbId,
        PlayerDbId,
        Do,
        DoMax,
        BuddyTemplateId,
        BuddyGachaName,
        Rarity,
        IsDuplicate,
        DuplicateRewardType,
        DuplicateRewardCount,
        UnixSeconds
    )
    SELECT
        BuddyGachaLogDbId,
        PlayerDbId,
        Do,
        DoMax,
        BuddyTemplateId,
        BuddyGachaName,
        Rarity,
        IsDuplicate,
        DuplicateRewardType,
        DuplicateRewardCount,
        UnixSeconds
    FROM BuddyGachaLog_Old;

    SET IDENTITY_INSERT BuddyGachaLog OFF;
");

            // 5. Drop old table
            migrationBuilder.DropTable("BuddyGachaLog_Old");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // 1. Recreate old table structure
            migrationBuilder.CreateTable(
                name: "BuddyGachaLog_Old",
                columns: table => new
                {
                    BuddyGachaLogDbId = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PlayerDbId = table.Column<int>(nullable: false),
                    Do = table.Column<int>(nullable: false),
                    DoMax = table.Column<int>(nullable: false),
                    BuddyTemplateId = table.Column<int>(nullable: false),
                    Rarity = table.Column<int>(nullable: false),
                    IsDuplicate = table.Column<bool>(nullable: false),
                    DuplicateRewardType = table.Column<int>(nullable: true),
                    DuplicateRewardCount = table.Column<int>(nullable: false),
                    UnixSeconds = table.Column<long>(nullable: false),
                    BuddyGachaName = table.Column<string>(nullable: false, defaultValue: "")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BuddyGachaLog_Old", x => x.BuddyGachaLogDbId);
                });

            // 2. Copy data back from current table
            migrationBuilder.Sql(@"
                INSERT INTO BuddyGachaLog_Old (
                    BuddyGachaLogDbId,
                    PlayerDbId,
                    Do,
                    DoMax,
                    BuddyTemplateId,
                    BuddyGachaName,
                    Rarity,
                    IsDuplicate,
                    DuplicateRewardType,
                    DuplicateRewardCount,
                    UnixSeconds
                )
                SELECT
                    BuddyGachaLogDbId,
                    PlayerDbId,
                    Do,
                    DoMax,
                    BuddyTemplateId,
                    BuddyGachaName,
                    Rarity,
                    IsDuplicate,
                    DuplicateRewardType,
                    DuplicateRewardCount,
                    UnixSeconds
                FROM BuddyGachaLog
            ");

            // 3. Drop current table
            migrationBuilder.DropTable("BuddyGachaLog");

            // 4. Rename old back to BuddyGachaLog
            migrationBuilder.RenameTable(
                name: "BuddyGachaLog_Old",
                newName: "BuddyGachaLog");

            // 5. Restore PK name
            migrationBuilder.DropPrimaryKey(
                name: "PK_BuddyGachaLog_Old",
                table: "BuddyGachaLog");

            migrationBuilder.AddPrimaryKey(
                name: "PK_BuddyGachaLog",
                table: "BuddyGachaLog",
                column: "BuddyGachaLogDbId");
        }
    }
}
