using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace BarberShop.Infra.Migrations
{
    /// <inheritdoc />
    public partial class SeedInitialData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Modules",
                columns: new[] { "Id", "CreationDate", "Edit", "Exclude", "ExclusionDate", "Inactivate", "Name", "Register", "UpdateDate", "Visualize" },
                values: new object[,]
                {
                    { 1L, null, true, true, null, true, "Users", true, null, true },
                    { 2L, null, true, true, null, true, "Auth", true, null, true },
                    { 3L, null, true, true, null, true, "SendEmail", true, null, true },
                    { 4L, null, true, true, null, true, "Appointments", true, null, true },
                    { 5L, null, true, true, null, true, "Payments", true, null, true }
                });

            migrationBuilder.InsertData(
                table: "Profiles",
                columns: new[] { "Id", "CreationDate", "ExclusionDate", "Name", "Status", "UpdateDate" },
                values: new object[,]
                {
                    { 1L, null, null, "Admin", 1, null },
                    { 2L, null, null, "Cliente", 1, null }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CreationDate", "Email", "ExclusionDate", "ImageUrl", "Name", "Password", "ProviderId", "Status", "UpdateDate" },
                values: new object[] { 1L, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "admin@barbershop.com", null, null, "Admin", "$2a$12$4sHQj3OUBgi.syi7oQwAf.TCbjhMzvqedwYnoVKPx5PVHR19siKKK", null, 1, null });

            migrationBuilder.InsertData(
                table: "ProfilesModules",
                columns: new[] { "ModuleId", "ProfileId", "Edit", "Exclude", "Inactivate", "Register", "Visualize" },
                values: new object[,]
                {
                    { 1L, 1L, true, true, true, true, true },
                    { 2L, 1L, true, true, true, true, true },
                    { 2L, 2L, false, false, false, true, true },
                    { 3L, 1L, true, true, true, true, true },
                    { 4L, 1L, true, true, true, true, true },
                    { 4L, 2L, false, false, true, true, true },
                    { 5L, 1L, true, true, true, true, true },
                    { 5L, 2L, false, false, false, true, true }
                });

            migrationBuilder.InsertData(
                table: "ProfilesUsers",
                columns: new[] { "ProfileId", "UserId" },
                values: new object[] { 1L, 1L });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "ProfilesModules",
                keyColumns: new[] { "ModuleId", "ProfileId" },
                keyValues: new object[] { 1L, 1L });

            migrationBuilder.DeleteData(
                table: "ProfilesModules",
                keyColumns: new[] { "ModuleId", "ProfileId" },
                keyValues: new object[] { 2L, 1L });

            migrationBuilder.DeleteData(
                table: "ProfilesModules",
                keyColumns: new[] { "ModuleId", "ProfileId" },
                keyValues: new object[] { 2L, 2L });

            migrationBuilder.DeleteData(
                table: "ProfilesModules",
                keyColumns: new[] { "ModuleId", "ProfileId" },
                keyValues: new object[] { 3L, 1L });

            migrationBuilder.DeleteData(
                table: "ProfilesModules",
                keyColumns: new[] { "ModuleId", "ProfileId" },
                keyValues: new object[] { 4L, 1L });

            migrationBuilder.DeleteData(
                table: "ProfilesModules",
                keyColumns: new[] { "ModuleId", "ProfileId" },
                keyValues: new object[] { 4L, 2L });

            migrationBuilder.DeleteData(
                table: "ProfilesModules",
                keyColumns: new[] { "ModuleId", "ProfileId" },
                keyValues: new object[] { 5L, 1L });

            migrationBuilder.DeleteData(
                table: "ProfilesModules",
                keyColumns: new[] { "ModuleId", "ProfileId" },
                keyValues: new object[] { 5L, 2L });

            migrationBuilder.DeleteData(
                table: "ProfilesUsers",
                keyColumns: new[] { "ProfileId", "UserId" },
                keyValues: new object[] { 1L, 1L });

            migrationBuilder.DeleteData(
                table: "Modules",
                keyColumn: "Id",
                keyValue: 1L);

            migrationBuilder.DeleteData(
                table: "Modules",
                keyColumn: "Id",
                keyValue: 2L);

            migrationBuilder.DeleteData(
                table: "Modules",
                keyColumn: "Id",
                keyValue: 3L);

            migrationBuilder.DeleteData(
                table: "Modules",
                keyColumn: "Id",
                keyValue: 4L);

            migrationBuilder.DeleteData(
                table: "Modules",
                keyColumn: "Id",
                keyValue: 5L);

            migrationBuilder.DeleteData(
                table: "Profiles",
                keyColumn: "Id",
                keyValue: 1L);

            migrationBuilder.DeleteData(
                table: "Profiles",
                keyColumn: "Id",
                keyValue: 2L);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1L);
        }
    }
}
