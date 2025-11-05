using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERecruitment.Web.Migrations
{
    /// <inheritdoc />
    public partial class FixAuditEntryPrimaryKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_AuditEntries",
                table: "AuditEntries");

            migrationBuilder.DropIndex(
                name: "IX_AuditEntries_JobApplicationId",
                table: "AuditEntries");

            migrationBuilder.AlterColumn<string>(
                name: "Action",
                table: "AuditEntries",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "Actor",
                table: "AuditEntries",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "AuditEntries",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            // Generate unique GUIDs for existing rows
            migrationBuilder.Sql(@"
                UPDATE AuditEntries
                SET Id = NEWID()
                WHERE Id = '00000000-0000-0000-0000-000000000000'
            ");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AuditEntries",
                table: "AuditEntries",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_AuditEntries_JobApplicationId_TimestampUtc",
                table: "AuditEntries",
                columns: new[] { "JobApplicationId", "TimestampUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_AuditEntries",
                table: "AuditEntries");

            migrationBuilder.DropIndex(
                name: "IX_AuditEntries_JobApplicationId_TimestampUtc",
                table: "AuditEntries");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "AuditEntries");

            migrationBuilder.AlterColumn<string>(
                name: "Actor",
                table: "AuditEntries",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Action",
                table: "AuditEntries",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AuditEntries",
                table: "AuditEntries",
                columns: new[] { "TimestampUtc", "Actor", "Action" });

            migrationBuilder.CreateIndex(
                name: "IX_AuditEntries_JobApplicationId",
                table: "AuditEntries",
                column: "JobApplicationId");
        }
    }
}
