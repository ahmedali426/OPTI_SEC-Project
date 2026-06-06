using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Opti_Sec_Backend.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class addImagenameInsideGateSession : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CapturedImageName",
                table: "GateSessions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "019d0158-5874-7a35-8457-344e6faccb52",
                column: "PasswordHash",
                value: "AQAAAAIAAYagAAAAEIXkLxWgSvc7bHJG3uebjGmcKN+fv2IIw4CC3M31auOvCje+eSCUMooGpi9zWlFMGg==");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CapturedImageName",
                table: "GateSessions");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "019d0158-5874-7a35-8457-344e6faccb52",
                column: "PasswordHash",
                value: "AQAAAAIAAYagAAAAEM9f6Vc3hCI1JIRNab/p2g79FPw1fPEAVzgfdM8Axn/6Ha6sGwuOeM6CQrNg8Afkcg==");
        }
    }
}
