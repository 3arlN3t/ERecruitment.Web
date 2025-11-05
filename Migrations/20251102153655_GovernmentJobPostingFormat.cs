using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERecruitment.Web.Migrations
{
    /// <inheritdoc />
    public partial class GovernmentJobPostingFormat : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "KillerQuestionAnswer",
                table: "JobApplications");

            migrationBuilder.DropColumn(
                name: "KillerQuestionFailed",
                table: "JobApplications");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "JobPostings",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Location",
                table: "JobPostings",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Department",
                table: "JobPostings",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "AdditionalNotes",
                table: "JobPostings",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ApplicationEmail",
                table: "JobPostings",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ApplicationMethod",
                table: "JobPostings",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Centre",
                table: "JobPostings",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CertificationRequirements",
                table: "JobPostings",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ClosingDate",
                table: "JobPostings",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateLastModified",
                table: "JobPostings",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "DatePosted",
                table: "JobPostings",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "DiversityNote",
                table: "JobPostings",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DutiesDescription",
                table: "JobPostings",
                type: "nvarchar(max)",
                maxLength: 10000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "EmploymentEquityNote",
                table: "JobPostings",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EnquiriesContactPerson",
                table: "JobPostings",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "EnquiriesEmail",
                table: "JobPostings",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EnquiriesPhone",
                table: "JobPostings",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ExperienceRequirements",
                table: "JobPostings",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "JobPostings",
                type: "bit",
                nullable: false,
                defaultValue: false);

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
                name: "PostNumber",
                table: "JobPostings",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PostedByUserId",
                table: "JobPostings",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Province",
                table: "JobPostings",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "QualificationRequirements",
                table: "JobPostings",
                type: "nvarchar(max)",
                maxLength: 5000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ReferenceNumber",
                table: "JobPostings",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SalaryDetails",
                table: "JobPostings",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SalaryRange",
                table: "JobPostings",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SalaryStructure",
                table: "JobPostings",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SkillsRequirements",
                table: "JobPostings",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AdditionalNotes",
                table: "JobPostings");

            migrationBuilder.DropColumn(
                name: "ApplicationEmail",
                table: "JobPostings");

            migrationBuilder.DropColumn(
                name: "ApplicationMethod",
                table: "JobPostings");

            migrationBuilder.DropColumn(
                name: "Centre",
                table: "JobPostings");

            migrationBuilder.DropColumn(
                name: "CertificationRequirements",
                table: "JobPostings");

            migrationBuilder.DropColumn(
                name: "ClosingDate",
                table: "JobPostings");

            migrationBuilder.DropColumn(
                name: "DateLastModified",
                table: "JobPostings");

            migrationBuilder.DropColumn(
                name: "DatePosted",
                table: "JobPostings");

            migrationBuilder.DropColumn(
                name: "DiversityNote",
                table: "JobPostings");

            migrationBuilder.DropColumn(
                name: "DutiesDescription",
                table: "JobPostings");

            migrationBuilder.DropColumn(
                name: "EmploymentEquityNote",
                table: "JobPostings");

            migrationBuilder.DropColumn(
                name: "EnquiriesContactPerson",
                table: "JobPostings");

            migrationBuilder.DropColumn(
                name: "EnquiriesEmail",
                table: "JobPostings");

            migrationBuilder.DropColumn(
                name: "EnquiriesPhone",
                table: "JobPostings");

            migrationBuilder.DropColumn(
                name: "ExperienceRequirements",
                table: "JobPostings");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "JobPostings");

            migrationBuilder.DropColumn(
                name: "KnowledgeRequirements",
                table: "JobPostings");

            migrationBuilder.DropColumn(
                name: "OtherRequirements",
                table: "JobPostings");

            migrationBuilder.DropColumn(
                name: "PostNumber",
                table: "JobPostings");

            migrationBuilder.DropColumn(
                name: "PostedByUserId",
                table: "JobPostings");

            migrationBuilder.DropColumn(
                name: "Province",
                table: "JobPostings");

            migrationBuilder.DropColumn(
                name: "QualificationRequirements",
                table: "JobPostings");

            migrationBuilder.DropColumn(
                name: "ReferenceNumber",
                table: "JobPostings");

            migrationBuilder.DropColumn(
                name: "SalaryDetails",
                table: "JobPostings");

            migrationBuilder.DropColumn(
                name: "SalaryRange",
                table: "JobPostings");

            migrationBuilder.DropColumn(
                name: "SalaryStructure",
                table: "JobPostings");

            migrationBuilder.DropColumn(
                name: "SkillsRequirements",
                table: "JobPostings");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "JobPostings",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(300)",
                oldMaxLength: 300);

            migrationBuilder.AlterColumn<string>(
                name: "Location",
                table: "JobPostings",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "Department",
                table: "JobPostings",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AddColumn<string>(
                name: "KillerQuestionAnswer",
                table: "JobApplications",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "KillerQuestionFailed",
                table: "JobApplications",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
