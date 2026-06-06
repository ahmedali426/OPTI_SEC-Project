using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Opti_Sec_Backend.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class updateimageinmembertabletobenotnull : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ImageUrl",
                table: "Members",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "019d0158-5874-7a35-8457-344e6faccb52",
                column: "PasswordHash",
                value: "AQAAAAIAAYagAAAAEEp9nvHKAuDnmYNQmYF7NHeMbGWq+7QfbSFyiMyOr/Uhmc6D+mGpeTr8eeUAsBkn+w==");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ImageUrl",
                table: "Members",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "019d0158-5874-7a35-8457-344e6faccb52",
                column: "PasswordHash",
                value: "AQAAAAIAAYagAAAAEAWS9bZMg3l2SpYhL3x4Pdn+Ne6y4qhjuSb4CKCZgOvyKZEka66jRi0vorAkzMrVug==");
        }
    }
}
