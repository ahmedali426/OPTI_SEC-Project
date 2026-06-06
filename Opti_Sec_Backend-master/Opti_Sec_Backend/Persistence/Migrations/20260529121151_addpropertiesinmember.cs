using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Opti_Sec_Backend.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class addpropertiesinmember : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AITrainingError",
                table: "Members",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "TrainingCompletedAt",
                table: "Members",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "TrainingStartedAt",
                table: "Members",
                type: "datetime2",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "019d0158-5874-7a35-8457-344e6faccb52",
                column: "PasswordHash",
                value: "AQAAAAIAAYagAAAAEDVylsUGw8H8jAT68whfwcTnYPsv7Y5j2fiPQm3cDRwrtwrId4QWZia1iObCQ1ERLQ==");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AITrainingError",
                table: "Members");

            migrationBuilder.DropColumn(
                name: "TrainingCompletedAt",
                table: "Members");

            migrationBuilder.DropColumn(
                name: "TrainingStartedAt",
                table: "Members");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "019d0158-5874-7a35-8457-344e6faccb52",
                column: "PasswordHash",
                value: "AQAAAAIAAYagAAAAENihKU2tvyGgP142z/30UMuf23zx+E9s8a1R+K0WIqKmj/gVpaAIBOr8FRLRwPQ3MA==");
        }
    }
}
