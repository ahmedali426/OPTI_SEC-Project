using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Opti_Sec_Backend.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAdminandRole : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "IsDefault", "IsDeleted", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "019d0165-8401-7242-8c9b-65e629be99b3", "019d0166-15af-7300-b2be-87428be3dd2e", false, false, "Admin", "ADMIN" },
                    { "019d0165-bdd2-79de-9c06-f98ae213063a", "019d0166-3bd5-7de0-bcb1-39b8bed0cbc7", true, false, "Client", "CLIENT" }
                });

            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "ConcurrencyStamp", "Discriminator", "Email", "EmailConfirmed", "FName", "LName", "LockoutEnabled", "LockoutEnd", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "SecurityStamp", "TwoFactorEnabled", "UserName" },
                values: new object[] { "019d0158-5874-7a35-8457-344e6faccb52", 0, "019d015a-a8bc-7bc7-88b7-63b06bd80e5b", "ApplicationUser", "admin@Opti_Sec2233.com", true, "Opti_Sec", "Admin", false, null, "ADMIN@OPTI_SEC2233.COM", "ADMIN@OPTI_SEC2233.COM", "AQAAAAIAAYagAAAAENaU2bhz9Tb9hTLtMN4CNE4JMSU8JCWGOSOEc6tTDVEGGjkcOzgFfKBSErbRndlpLw==", null, false, "019d015a5c8b795abcf898945cc9a359", false, "admin@Opti_Sec2233.com" });

            migrationBuilder.InsertData(
                table: "AspNetUserRoles",
                columns: new[] { "RoleId", "UserId" },
                values: new object[] { "019d0165-8401-7242-8c9b-65e629be99b3", "019d0158-5874-7a35-8457-344e6faccb52" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "019d0165-bdd2-79de-9c06-f98ae213063a");

            migrationBuilder.DeleteData(
                table: "AspNetUserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { "019d0165-8401-7242-8c9b-65e629be99b3", "019d0158-5874-7a35-8457-344e6faccb52" });

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "019d0165-8401-7242-8c9b-65e629be99b3");

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "019d0158-5874-7a35-8457-344e6faccb52");
        }
    }
}
