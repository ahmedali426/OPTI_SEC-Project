using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Opti_Sec_Backend.Persistence.MigrationsDeployment
{
    /// <inheritdoc />
    public partial class deployinit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "019d0158-5874-7a35-8457-344e6faccb52",
                column: "PasswordHash",
                value: "AQAAAAIAAYagAAAAEKk4e2mkaNp0xBYhT/CQFZFD0ZZroLtNcTXH7IEqIHEaRmg+411uldab2jqfN9chWA==");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "019d0158-5874-7a35-8457-344e6faccb52",
                column: "PasswordHash",
                value: "AQAAAAIAAYagAAAAEAOLhikYFhBhIYpOH3KYhP9kZOitMqm27cMg/YGx251p0+h5CqCxqdUzL/eHz2dqsg==");
        }
    }
}
