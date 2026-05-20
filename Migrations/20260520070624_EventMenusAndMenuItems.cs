using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WeddingPlannerApp.Migrations
{
    /// <inheritdoc />
    public partial class EventMenusAndMenuItems : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EventMenus",
                columns: table => new
                {
                    EventId = table.Column<int>(type: "INTEGER", nullable: false),
                    MenuId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventMenus", x => new { x.EventId, x.MenuId });
                    table.ForeignKey(
                        name: "FK_EventMenus_Events_EventId",
                        column: x => x.EventId,
                        principalTable: "Events",
                        principalColumn: "EventId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EventMenus_Menus_MenuId",
                        column: x => x.MenuId,
                        principalTable: "Menus",
                        principalColumn: "MenuId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MenuItems",
                columns: table => new
                {
                    MenuItemId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MenuId = table.Column<int>(type: "INTEGER", nullable: false),
                    CourseName = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    DisplayOrder = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MenuItems", x => x.MenuItemId);
                    table.ForeignKey(
                        name: "FK_MenuItems_Menus_MenuId",
                        column: x => x.MenuId,
                        principalTable: "Menus",
                        principalColumn: "MenuId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EventMenus_MenuId",
                table: "EventMenus",
                column: "MenuId");

            migrationBuilder.CreateIndex(
                name: "IX_MenuItems_MenuId",
                table: "MenuItems",
                column: "MenuId");

            migrationBuilder.Sql("""
                INSERT INTO EventMenus (EventId, MenuId)
                SELECT EventId, MenuId
                FROM Events
                WHERE MenuId IS NOT NULL
                """);

            migrationBuilder.DropForeignKey(
                name: "FK_Events_Menus_MenuId",
                table: "Events");

            migrationBuilder.DropIndex(
                name: "IX_Events_MenuId",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "MenuId",
                table: "Events");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MenuId",
                table: "Events",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql("""
                UPDATE Events
                SET MenuId = COALESCE((
                    SELECT MenuId
                    FROM EventMenus
                    WHERE EventMenus.EventId = Events.EventId
                    ORDER BY MenuId
                    LIMIT 1
                ), (
                    SELECT MenuId
                    FROM Menus
                    ORDER BY MenuId
                    LIMIT 1
                ), 0)
                """);

            migrationBuilder.DropTable(
                name: "EventMenus");

            migrationBuilder.DropTable(
                name: "MenuItems");

            migrationBuilder.CreateIndex(
                name: "IX_Events_MenuId",
                table: "Events",
                column: "MenuId");

            migrationBuilder.AddForeignKey(
                name: "FK_Events_Menus_MenuId",
                table: "Events",
                column: "MenuId",
                principalTable: "Menus",
                principalColumn: "MenuId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
