using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERecruitment.Web.Migrations
{
    /// <inheritdoc />
    public partial class ConsolidateRequirementsFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CertificationRequirements",
                table: "JobPostings");

            migrationBuilder.DropColumn(
                name: "ExperienceRequirements",
                table: "JobPostings");

            migrationBuilder.DropColumn(
                name: "KnowledgeRequirements",
                table: "JobPostings");

            migrationBuilder.DropColumn(
                name: "OtherRequirements",
                table: "JobPostings");

            migrationBuilder.DropColumn(
                name: "QualificationRequirements",
                table: "JobPostings");

            migrationBuilder.DropColumn(
                name: "SkillsRequirements",
                table: "JobPostings");

            migrationBuilder.AddColumn<string>(
                name: "Requirements",
                table: "JobPostings",
                type: "nvarchar(max)",
                maxLength: 10000,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Requirements",
                table: "JobPostings");

            migrationBuilder.AddColumn<string>(
                name: "CertificationRequirements",
                table: "JobPostings",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExperienceRequirements",
                table: "JobPostings",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "KnowledgeRequirements",
                table: "JobPostings",
                type: "nvarchar(3000)",
                maxLength: 3000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OtherRequirements",
                table: "JobPostings",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "QualificationRequirements",
                table: "JobPostings",
                type: "nvarchar(max)",
                maxLength: 5000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SkillsRequirements",
                table: "JobPostings",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);
        }
    }
}
