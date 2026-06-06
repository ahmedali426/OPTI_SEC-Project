using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Opti_Sec_Backend.Persistence.MigrationsDeployment
{
    /// <inheritdoc />
    public partial class makeondeletecascade : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AccessLogs_Gates_GateId",
                table: "AccessLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_AccessLogs_Members_MemberId",
                table: "AccessLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_Gates_Clients_ClientId",
                table: "Gates");

            migrationBuilder.AlterColumn<int>(
                name: "GateId",
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
                value: "AQAAAAIAAYagAAAAEFLE3hEtOBQsQZnTjEqiD6xwRq6DBpPKUDVLfOWY/WN0cxCbcNmPDfPhRbHVl8h4ew==");

            migrationBuilder.AddForeignKey(
                name: "FK_AccessLogs_Gates_GateId",
                table: "AccessLogs",
                column: "GateId",
                principalTable: "Gates",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AccessLogs_Members_MemberId",
                table: "AccessLogs",
                column: "MemberId",
                principalTable: "Members",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Gates_Clients_ClientId",
                table: "Gates",
                column: "ClientId",
                principalTable: "Clients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AccessLogs_Gates_GateId",
                table: "AccessLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_AccessLogs_Members_MemberId",
                table: "AccessLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_Gates_Clients_ClientId",
                table: "Gates");

            migrationBuilder.AlterColumn<int>(
                name: "GateId",
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
                value: "AQAAAAIAAYagAAAAEKk4e2mkaNp0xBYhT/CQFZFD0ZZroLtNcTXH7IEqIHEaRmg+411uldab2jqfN9chWA==");

            migrationBuilder.AddForeignKey(
                name: "FK_AccessLogs_Gates_GateId",
                table: "AccessLogs",
                column: "GateId",
                principalTable: "Gates",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AccessLogs_Members_MemberId",
                table: "AccessLogs",
                column: "MemberId",
                principalTable: "Members",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Gates_Clients_ClientId",
                table: "Gates",
                column: "ClientId",
                principalTable: "Clients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
