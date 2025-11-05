using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERecruitment.Web.Migrations
{
    /// <inheritdoc />
    public partial class ConsolidateSalaryFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SalaryDetails",
                table: "JobPostings");

            migrationBuilder.DropColumn(
                name: "SalaryStructure",
                table: "JobPostings");

            migrationBuilder.AlterColumn<string>(
                name: "SalaryRange",
                table: "JobPostings",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "SalaryRange",
                table: "JobPostings",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(2000)",
                oldMaxLength: 2000);

            migrationBuilder.AddColumn<string>(
                name: "SalaryDetails",
                table: "JobPostings",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SalaryStructure",
                table: "JobPostings",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);
        }
    }
}
