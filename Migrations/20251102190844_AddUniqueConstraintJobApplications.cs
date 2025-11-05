using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERecruitment.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueConstraintJobApplications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_JobApplications_ApplicantId",
                table: "JobApplications");

            migrationBuilder.CreateIndex(
                name: "IX_JobApplications_ApplicantId_JobPostingId_Unique",
                table: "JobApplications",
                columns: new[] { "ApplicantId", "JobPostingId" },
                unique: true,
                filter: "[Status] != 5");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_JobApplications_ApplicantId_JobPostingId_Unique",
                table: "JobApplications");

            migrationBuilder.CreateIndex(
                name: "IX_JobApplications_ApplicantId",
                table: "JobApplications",
                column: "ApplicantId");
        }
    }
}
