using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace BarberShop.Infra.Migrations
{
    /// <inheritdoc />
    public partial class AddBarberEntityAndSeedBarberProfile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Barbers",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    DisplayName = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreationDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    UpdateDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    ExclusionDate = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Barbers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Barbers_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.InsertData(
                table: "Modules",
                columns: new[] { "Id", "CreationDate", "Edit", "Exclude", "ExclusionDate", "Inactivate", "Name", "Register", "UpdateDate", "Visualize" },
                values: new object[] { 6L, null, true, true, null, true, "Barber", true, null, true });

            migrationBuilder.InsertData(
                table: "Profiles",
                columns: new[] { "Id", "CreationDate", "ExclusionDate", "Name", "Status", "UpdateDate" },
                values: new object[] { 3L, null, null, "Barbeiro", 1, null });

            migrationBuilder.InsertData(
                table: "ProfilesModules",
                columns: new[] { "ModuleId", "ProfileId", "Edit", "Exclude", "Inactivate", "Register", "Visualize" },
                values: new object[,]
                {
                    { 4L, 3L, false, false, true, true, true },
                    { 6L, 3L, true, true, true, true, true }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Barbers_UserId",
                table: "Barbers",
                column: "UserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Barbers");

            migrationBuilder.DeleteData(
                table: "ProfilesModules",
                keyColumns: new[] { "ModuleId", "ProfileId" },
                keyValues: new object[] { 4L, 3L });

            migrationBuilder.DeleteData(
                table: "ProfilesModules",
                keyColumns: new[] { "ModuleId", "ProfileId" },
                keyValues: new object[] { 6L, 3L });

            migrationBuilder.DeleteData(
                table: "Modules",
                keyColumn: "Id",
                keyValue: 6L);

            migrationBuilder.DeleteData(
                table: "Profiles",
                keyColumn: "Id",
                keyValue: 3L);
        }
    }
}
