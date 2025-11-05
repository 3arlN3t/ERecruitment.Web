using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERecruitment.Web.Migrations
{
    /// <inheritdoc />
    public partial class RemoveEnquiriesEmail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EnquiriesEmail",
                table: "JobPostings");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EnquiriesEmail",
                table: "JobPostings",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);
        }
    }
}
