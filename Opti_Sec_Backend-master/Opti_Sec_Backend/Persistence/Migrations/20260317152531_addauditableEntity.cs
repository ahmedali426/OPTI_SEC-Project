using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Opti_Sec_Backend.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class addauditableEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AccessLogs_Members_MemberId",
                table: "AccessLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_Gates_AspNetUsers_ClientId",
                table: "Gates");

            migrationBuilder.DropForeignKey(
                name: "FK_Members_AspNetUsers_ClientId",
                table: "Members");

            migrationBuilder.AddColumn<string>(
                name: "CreatedById",
                table: "Members",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedOn",
                table: "Members",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "UpdatedById",
                table: "Members",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedOn",
                table: "Members",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedById",
                table: "Gates",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedOn",
                table: "Gates",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "UpdatedById",
                table: "Gates",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedOn",
                table: "Gates",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedById",
                table: "AccessLogs",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedOn",
                table: "AccessLogs",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "UpdatedById",
                table: "AccessLogs",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedOn",
                table: "AccessLogs",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Members_CreatedById",
                table: "Members",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Members_UpdatedById",
                table: "Members",
                column: "UpdatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Gates_CreatedById",
                table: "Gates",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Gates_UpdatedById",
                table: "Gates",
                column: "UpdatedById");

            migrationBuilder.CreateIndex(
                name: "IX_AccessLogs_CreatedById",
                table: "AccessLogs",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_AccessLogs_UpdatedById",
                table: "AccessLogs",
                column: "UpdatedById");

            migrationBuilder.AddForeignKey(
                name: "FK_AccessLogs_AspNetUsers_CreatedById",
                table: "AccessLogs",
                column: "CreatedById",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AccessLogs_AspNetUsers_UpdatedById",
                table: "AccessLogs",
                column: "UpdatedById",
                principalTable: "AspNetUsers",
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
                name: "FK_Gates_AspNetUsers_ClientId",
                table: "Gates",
                column: "ClientId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Gates_AspNetUsers_CreatedById",
                table: "Gates",
                column: "CreatedById",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Gates_AspNetUsers_UpdatedById",
                table: "Gates",
                column: "UpdatedById",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Members_AspNetUsers_ClientId",
                table: "Members",
                column: "ClientId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Members_AspNetUsers_CreatedById",
                table: "Members",
                column: "CreatedById",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Members_AspNetUsers_UpdatedById",
                table: "Members",
                column: "UpdatedById",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AccessLogs_AspNetUsers_CreatedById",
                table: "AccessLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_AccessLogs_AspNetUsers_UpdatedById",
                table: "AccessLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_AccessLogs_Members_MemberId",
                table: "AccessLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_Gates_AspNetUsers_ClientId",
                table: "Gates");

            migrationBuilder.DropForeignKey(
                name: "FK_Gates_AspNetUsers_CreatedById",
                table: "Gates");

            migrationBuilder.DropForeignKey(
                name: "FK_Gates_AspNetUsers_UpdatedById",
                table: "Gates");

            migrationBuilder.DropForeignKey(
                name: "FK_Members_AspNetUsers_ClientId",
                table: "Members");

            migrationBuilder.DropForeignKey(
                name: "FK_Members_AspNetUsers_CreatedById",
                table: "Members");

            migrationBuilder.DropForeignKey(
                name: "FK_Members_AspNetUsers_UpdatedById",
                table: "Members");

            migrationBuilder.DropIndex(
                name: "IX_Members_CreatedById",
                table: "Members");

            migrationBuilder.DropIndex(
                name: "IX_Members_UpdatedById",
                table: "Members");

            migrationBuilder.DropIndex(
                name: "IX_Gates_CreatedById",
                table: "Gates");

            migrationBuilder.DropIndex(
                name: "IX_Gates_UpdatedById",
                table: "Gates");

            migrationBuilder.DropIndex(
                name: "IX_AccessLogs_CreatedById",
                table: "AccessLogs");

            migrationBuilder.DropIndex(
                name: "IX_AccessLogs_UpdatedById",
                table: "AccessLogs");

            migrationBuilder.DropColumn(
                name: "CreatedById",
                table: "Members");

            migrationBuilder.DropColumn(
                name: "CreatedOn",
                table: "Members");

            migrationBuilder.DropColumn(
                name: "UpdatedById",
                table: "Members");

            migrationBuilder.DropColumn(
                name: "UpdatedOn",
                table: "Members");

            migrationBuilder.DropColumn(
                name: "CreatedById",
                table: "Gates");

            migrationBuilder.DropColumn(
                name: "CreatedOn",
                table: "Gates");

            migrationBuilder.DropColumn(
                name: "UpdatedById",
                table: "Gates");

            migrationBuilder.DropColumn(
                name: "UpdatedOn",
                table: "Gates");

            migrationBuilder.DropColumn(
                name: "CreatedById",
                table: "AccessLogs");

            migrationBuilder.DropColumn(
                name: "CreatedOn",
                table: "AccessLogs");

            migrationBuilder.DropColumn(
                name: "UpdatedById",
                table: "AccessLogs");

            migrationBuilder.DropColumn(
                name: "UpdatedOn",
                table: "AccessLogs");

            migrationBuilder.AddForeignKey(
                name: "FK_AccessLogs_Members_MemberId",
                table: "AccessLogs",
                column: "MemberId",
                principalTable: "Members",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Gates_AspNetUsers_ClientId",
                table: "Gates",
                column: "ClientId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Members_AspNetUsers_ClientId",
                table: "Members",
                column: "ClientId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
