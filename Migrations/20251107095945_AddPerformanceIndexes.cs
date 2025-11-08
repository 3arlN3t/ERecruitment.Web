using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERecruitment.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddPerformanceIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ToEmail",
                table: "EmailDeliveries",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Applicants",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_JobApplications_Posting_Status_SubmittedAt",
                table: "JobApplications",
                columns: new[] { "JobPostingId", "Status", "SubmittedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_EmailDeliveries_ToEmail_Timestamp",
                table: "EmailDeliveries",
                columns: new[] { "ToEmail", "TimestampUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_Applicants_Email",
                table: "Applicants",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_JobApplications_Posting_Status_SubmittedAt",
                table: "JobApplications");

            migrationBuilder.DropIndex(
                name: "IX_EmailDeliveries_ToEmail_Timestamp",
                table: "EmailDeliveries");

            migrationBuilder.DropIndex(
                name: "IX_Applicants_Email",
                table: "Applicants");

            migrationBuilder.AlterColumn<string>(
                name: "ToEmail",
                table: "EmailDeliveries",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Applicants",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");
        }
    }
}
