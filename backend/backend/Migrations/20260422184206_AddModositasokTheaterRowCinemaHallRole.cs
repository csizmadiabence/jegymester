using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class AddModositasokTheaterRowCinemaHallRole : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Role",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "RoomName",
                table: "Screenings");

            migrationBuilder.AddColumn<int>(
                name: "TheaterRowId",
                table: "Seats",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CinemaHallId",
                table: "Screenings",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "CinemaHalls",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CinemaHalls", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TheaterRows",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RowNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    CinemaHallId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TheaterRows", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TheaterRows_CinemaHalls_CinemaHallId",
                        column: x => x.CinemaHallId,
                        principalTable: "CinemaHalls",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "RoleUser",
                columns: table => new
                {
                    RolesId = table.Column<int>(type: "INTEGER", nullable: false),
                    UsersId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleUser", x => new { x.RolesId, x.UsersId });
                    table.ForeignKey(
                        name: "FK_RoleUser_Roles_RolesId",
                        column: x => x.RolesId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RoleUser_Users_UsersId",
                        column: x => x.UsersId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Admin" },
                    { 2, "Cashier" },
                    { 3, "User" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Seats_TheaterRowId",
                table: "Seats",
                column: "TheaterRowId");

            migrationBuilder.CreateIndex(
                name: "IX_Screenings_CinemaHallId",
                table: "Screenings",
                column: "CinemaHallId");

            migrationBuilder.CreateIndex(
                name: "IX_RoleUser_UsersId",
                table: "RoleUser",
                column: "UsersId");

            migrationBuilder.CreateIndex(
                name: "IX_TheaterRows_CinemaHallId",
                table: "TheaterRows",
                column: "CinemaHallId");

            migrationBuilder.AddForeignKey(
                name: "FK_Screenings_CinemaHalls_CinemaHallId",
                table: "Screenings",
                column: "CinemaHallId",
                principalTable: "CinemaHalls",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Seats_TheaterRows_TheaterRowId",
                table: "Seats",
                column: "TheaterRowId",
                principalTable: "TheaterRows",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Screenings_CinemaHalls_CinemaHallId",
                table: "Screenings");

            migrationBuilder.DropForeignKey(
                name: "FK_Seats_TheaterRows_TheaterRowId",
                table: "Seats");

            migrationBuilder.DropTable(
                name: "RoleUser");

            migrationBuilder.DropTable(
                name: "TheaterRows");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "CinemaHalls");

            migrationBuilder.DropIndex(
                name: "IX_Seats_TheaterRowId",
                table: "Seats");

            migrationBuilder.DropIndex(
                name: "IX_Screenings_CinemaHallId",
                table: "Screenings");

            migrationBuilder.DropColumn(
                name: "TheaterRowId",
                table: "Seats");

            migrationBuilder.DropColumn(
                name: "CinemaHallId",
                table: "Screenings");

            migrationBuilder.AddColumn<int>(
                name: "Role",
                table: "Users",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "RoomName",
                table: "Screenings",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }
    }
}
