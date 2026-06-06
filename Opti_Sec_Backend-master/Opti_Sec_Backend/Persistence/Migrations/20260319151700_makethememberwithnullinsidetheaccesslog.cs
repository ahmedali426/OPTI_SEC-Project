using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Opti_Sec_Backend.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class makethememberwithnullinsidetheaccesslog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "MemberId",
                table: "AccessLogs",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "019d0158-5874-7a35-8457-344e6faccb52",
                column: "PasswordHash",
                value: "AQAAAAIAAYagAAAAEAWS9bZMg3l2SpYhL3x4Pdn+Ne6y4qhjuSb4CKCZgOvyKZEka66jRi0vorAkzMrVug==");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "MemberId",
                table: "AccessLogs",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "019d0158-5874-7a35-8457-344e6faccb52",
                column: "PasswordHash",
                value: "AQAAAAIAAYagAAAAEK0MzPX87/YjVCToTT9O4PpaZHe0eZ/fUZTD9merpJZJvCa/e2koR2OUE3pDZ/w1sA==");
        }
    }
}
