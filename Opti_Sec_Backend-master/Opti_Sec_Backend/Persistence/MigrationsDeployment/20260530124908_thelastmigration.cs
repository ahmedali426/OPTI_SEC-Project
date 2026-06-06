using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Opti_Sec_Backend.Persistence.MigrationsDeployment
{
    /// <inheritdoc />
    public partial class thelastmigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "019d0158-5874-7a35-8457-344e6faccb52",
                column: "PasswordHash",
                value: "AQAAAAIAAYagAAAAEGlbpPxuakXIHPxF2TKSDld3f1ShJTnrt3rz7Ti/BIJQNYN0p5yVAvO6lfvjwP3YTQ==");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "019d0158-5874-7a35-8457-344e6faccb52",
                column: "PasswordHash",
                value: "AQAAAAIAAYagAAAAEIXkLxWgSvc7bHJG3uebjGmcKN+fv2IIw4CC3M31auOvCje+eSCUMooGpi9zWlFMGg==");
        }
    }
}
