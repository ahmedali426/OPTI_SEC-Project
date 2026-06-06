using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Opti_Sec_Backend.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddtwoPropertyInsideGate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FailedAttemptsCount",
                table: "Gates",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastFailedAttemptAt",
                table: "Gates",
                type: "datetime2",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "019d0158-5874-7a35-8457-344e6faccb52",
                column: "PasswordHash",
                value: "AQAAAAIAAYagAAAAELW18iodOfzzNVWLP+CkrT5bjPv0eGQL2/A76dEZEmDSUn7ielXOYZCnrSaNlqKAxw==");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FailedAttemptsCount",
                table: "Gates");

            migrationBuilder.DropColumn(
                name: "LastFailedAttemptAt",
                table: "Gates");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "019d0158-5874-7a35-8457-344e6faccb52",
                column: "PasswordHash",
                value: "AQAAAAIAAYagAAAAEGlbpPxuakXIHPxF2TKSDld3f1ShJTnrt3rz7Ti/BIJQNYN0p5yVAvO6lfvjwP3YTQ==");
        }
    }
}
