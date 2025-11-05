using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERecruitment.Web.Migrations
{
    /// <inheritdoc />
    public partial class Z83Expansion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Profile_Skills",
                table: "Applicants");

            migrationBuilder.DropColumn(
                name: "Profile_WorkHistory",
                table: "Applicants");

            migrationBuilder.AddColumn<string>(
                name: "Profile_AdditionalDocument_ContentType",
                table: "Applicants",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Profile_AdditionalDocument_DocumentType",
                table: "Applicants",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Profile_AdditionalDocument_FileName",
                table: "Applicants",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Profile_AdditionalDocument_StorageToken",
                table: "Applicants",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Profile_AdditionalDocument_UploadedAtUtc",
                table: "Applicants",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Profile_AvailabilityDate",
                table: "Applicants",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Profile_AvailabilityNotice",
                table: "Applicants",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Profile_BusinessDetails",
                table: "Applicants",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Profile_BusinessWithState",
                table: "Applicants",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Profile_ContactEmail",
                table: "Applicants",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Profile_CriminalRecordDetails",
                table: "Applicants",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Profile_DateOfBirth",
                table: "Applicants",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Profile_DeclarationAccepted",
                table: "Applicants",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "Profile_DeclarationDate",
                table: "Applicants",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Profile_DepartmentName",
                table: "Applicants",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Profile_DisabilityDetails",
                table: "Applicants",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Profile_DischargedDetails",
                table: "Applicants",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Profile_DischargedForIllHealth",
                table: "Applicants",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Profile_DismissedDetails",
                table: "Applicants",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Profile_DismissedForMisconduct",
                table: "Applicants",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Profile_DriversLicenseDocument_ContentType",
                table: "Applicants",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Profile_DriversLicenseDocument_DocumentType",
                table: "Applicants",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Profile_DriversLicenseDocument_FileName",
                table: "Applicants",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Profile_DriversLicenseDocument_StorageToken",
                table: "Applicants",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Profile_DriversLicenseDocument_UploadedAtUtc",
                table: "Applicants",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Profile_HasCriminalRecord",
                table: "Applicants",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Profile_HasDisability",
                table: "Applicants",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Profile_HasPendingCase",
                table: "Applicants",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Profile_HasWorkPermit",
                table: "Applicants",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Profile_IdDocument_ContentType",
                table: "Applicants",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Profile_IdDocument_DocumentType",
                table: "Applicants",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Profile_IdDocument_FileName",
                table: "Applicants",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Profile_IdDocument_StorageToken",
                table: "Applicants",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Profile_IdDocument_UploadedAtUtc",
                table: "Applicants",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Profile_IsSouthAfrican",
                table: "Applicants",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Profile_Nationality",
                table: "Applicants",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Profile_PassportNumber",
                table: "Applicants",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Profile_PendingCaseDetails",
                table: "Applicants",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Profile_PendingDisciplinaryCase",
                table: "Applicants",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Profile_PendingDisciplinaryDetails",
                table: "Applicants",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Profile_PositionName",
                table: "Applicants",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Profile_PreferredLanguage",
                table: "Applicants",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Profile_PrivateSectorYears",
                table: "Applicants",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Profile_ProfessionalInstitution",
                table: "Applicants",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Profile_ProfessionalRegistrationDate",
                table: "Applicants",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Profile_ProfessionalRegistrationNumber",
                table: "Applicants",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Profile_PublicSectorYears",
                table: "Applicants",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Profile_QualificationDocument_ContentType",
                table: "Applicants",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Profile_QualificationDocument_DocumentType",
                table: "Applicants",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Profile_QualificationDocument_FileName",
                table: "Applicants",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Profile_QualificationDocument_StorageToken",
                table: "Applicants",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Profile_QualificationDocument_UploadedAtUtc",
                table: "Applicants",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Profile_ReappointmentCondition",
                table: "Applicants",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Profile_ReappointmentConditionDetails",
                table: "Applicants",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Profile_ReappointmentDepartment",
                table: "Applicants",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Profile_ReferenceNumber",
                table: "Applicants",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Profile_ResignedPendingDisciplinary",
                table: "Applicants",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Profile_ResignedPendingDisciplinaryDetails",
                table: "Applicants",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Profile_SignatureData",
                table: "Applicants",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Profile_WillRelinquishBusiness",
                table: "Applicants",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Profile_WorkPermitDetails",
                table: "Applicants",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "LanguageProficiency",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ApplicantProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LanguageName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SpeakProficiency = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ReadWriteProficiency = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LanguageProficiency", x => new { x.ApplicantProfileId, x.Id });
                    table.ForeignKey(
                        name: "FK_LanguageProficiency_Applicants_ApplicantProfileId",
                        column: x => x.ApplicantProfileId,
                        principalTable: "Applicants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QualificationRecord",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ApplicantProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InstitutionName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    QualificationName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StudentNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    YearObtained = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QualificationRecord", x => new { x.ApplicantProfileId, x.Id });
                    table.ForeignKey(
                        name: "FK_QualificationRecord_Applicants_ApplicantProfileId",
                        column: x => x.ApplicantProfileId,
                        principalTable: "Applicants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ReferenceContact",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ApplicantProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Relationship = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ContactNumber = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReferenceContact", x => new { x.ApplicantProfileId, x.Id });
                    table.ForeignKey(
                        name: "FK_ReferenceContact_Applicants_ApplicantProfileId",
                        column: x => x.ApplicantProfileId,
                        principalTable: "Applicants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkExperienceRecord",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ApplicantProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmployerName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PositionHeld = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FromDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ToDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ReasonForLeaving = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkExperienceRecord", x => new { x.ApplicantProfileId, x.Id });
                    table.ForeignKey(
                        name: "FK_WorkExperienceRecord_Applicants_ApplicantProfileId",
                        column: x => x.ApplicantProfileId,
                        principalTable: "Applicants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LanguageProficiency");

            migrationBuilder.DropTable(
                name: "QualificationRecord");

            migrationBuilder.DropTable(
                name: "ReferenceContact");

            migrationBuilder.DropTable(
                name: "WorkExperienceRecord");

            migrationBuilder.DropColumn(
                name: "Profile_AdditionalDocument_ContentType",
                table: "Applicants");

            migrationBuilder.DropColumn(
                name: "Profile_AdditionalDocument_DocumentType",
                table: "Applicants");

            migrationBuilder.DropColumn(
                name: "Profile_AdditionalDocument_FileName",
                table: "Applicants");

            migrationBuilder.DropColumn(
                name: "Profile_AdditionalDocument_StorageToken",
                table: "Applicants");

            migrationBuilder.DropColumn(
                name: "Profile_AdditionalDocument_UploadedAtUtc",
                table: "Applicants");

            migrationBuilder.DropColumn(
                name: "Profile_AvailabilityDate",
                table: "Applicants");

            migrationBuilder.DropColumn(
                name: "Profile_AvailabilityNotice",
                table: "Applicants");

            migrationBuilder.DropColumn(
                name: "Profile_BusinessDetails",
                table: "Applicants");

            migrationBuilder.DropColumn(
                name: "Profile_BusinessWithState",
                table: "Applicants");

            migrationBuilder.DropColumn(
                name: "Profile_ContactEmail",
                table: "Applicants");

            migrationBuilder.DropColumn(
                name: "Profile_CriminalRecordDetails",
                table: "Applicants");

            migrationBuilder.DropColumn(
                name: "Profile_DateOfBirth",
                table: "Applicants");

            migrationBuilder.DropColumn(
                name: "Profile_DeclarationAccepted",
                table: "Applicants");

            migrationBuilder.DropColumn(
                name: "Profile_DeclarationDate",
                table: "Applicants");

            migrationBuilder.DropColumn(
                name: "Profile_DepartmentName",
                table: "Applicants");

            migrationBuilder.DropColumn(
                name: "Profile_DisabilityDetails",
                table: "Applicants");

            migrationBuilder.DropColumn(
                name: "Profile_DischargedDetails",
                table: "Applicants");

            migrationBuilder.DropColumn(
                name: "Profile_DischargedForIllHealth",
                table: "Applicants");

            migrationBuilder.DropColumn(
                name: "Profile_DismissedDetails",
                table: "Applicants");

            migrationBuilder.DropColumn(
                name: "Profile_DismissedForMisconduct",
                table: "Applicants");

            migrationBuilder.DropColumn(
                name: "Profile_DriversLicenseDocument_ContentType",
                table: "Applicants");

            migrationBuilder.DropColumn(
                name: "Profile_DriversLicenseDocument_DocumentType",
                table: "Applicants");

            migrationBuilder.DropColumn(
                name: "Profile_DriversLicenseDocument_FileName",
                table: "Applicants");

            migrationBuilder.DropColumn(
                name: "Profile_DriversLicenseDocument_StorageToken",
                table: "Applicants");

            migrationBuilder.DropColumn(
                name: "Profile_DriversLicenseDocument_UploadedAtUtc",
                table: "Applicants");

            migrationBuilder.DropColumn(
                name: "Profile_HasCriminalRecord",
                table: "Applicants");

            migrationBuilder.DropColumn(
                name: "Profile_HasDisability",
                table: "Applicants");

            migrationBuilder.DropColumn(
                name: "Profile_HasPendingCase",
                table: "Applicants");

            migrationBuilder.DropColumn(
                name: "Profile_HasWorkPermit",
                table: "Applicants");

            migrationBuilder.DropColumn(
                name: "Profile_IdDocument_ContentType",
                table: "Applicants");

            migrationBuilder.DropColumn(
                name: "Profile_IdDocument_DocumentType",
                table: "Applicants");

            migrationBuilder.DropColumn(
                name: "Profile_IdDocument_FileName",
                table: "Applicants");

            migrationBuilder.DropColumn(
                name: "Profile_IdDocument_StorageToken",
                table: "Applicants");

            migrationBuilder.DropColumn(
                name: "Profile_IdDocument_UploadedAtUtc",
                table: "Applicants");

            migrationBuilder.DropColumn(
                name: "Profile_IsSouthAfrican",
                table: "Applicants");

            migrationBuilder.DropColumn(
                name: "Profile_Nationality",
                table: "Applicants");

            migrationBuilder.DropColumn(
                name: "Profile_PassportNumber",
                table: "Applicants");

            migrationBuilder.DropColumn(
                name: "Profile_PendingCaseDetails",
                table: "Applicants");

            migrationBuilder.DropColumn(
                name: "Profile_PendingDisciplinaryCase",
                table: "Applicants");

            migrationBuilder.DropColumn(
                name: "Profile_PendingDisciplinaryDetails",
                table: "Applicants");

            migrationBuilder.DropColumn(
                name: "Profile_PositionName",
                table: "Applicants");

            migrationBuilder.DropColumn(
                name: "Profile_PreferredLanguage",
                table: "Applicants");

            migrationBuilder.DropColumn(
                name: "Profile_PrivateSectorYears",
                table: "Applicants");

            migrationBuilder.DropColumn(
                name: "Profile_ProfessionalInstitution",
                table: "Applicants");

            migrationBuilder.DropColumn(
                name: "Profile_ProfessionalRegistrationDate",
                table: "Applicants");

            migrationBuilder.DropColumn(
                name: "Profile_ProfessionalRegistrationNumber",
                table: "Applicants");

            migrationBuilder.DropColumn(
                name: "Profile_PublicSectorYears",
                table: "Applicants");

            migrationBuilder.DropColumn(
                name: "Profile_QualificationDocument_ContentType",
                table: "Applicants");

            migrationBuilder.DropColumn(
                name: "Profile_QualificationDocument_DocumentType",
                table: "Applicants");

            migrationBuilder.DropColumn(
                name: "Profile_QualificationDocument_FileName",
                table: "Applicants");

            migrationBuilder.DropColumn(
                name: "Profile_QualificationDocument_StorageToken",
                table: "Applicants");

            migrationBuilder.DropColumn(
                name: "Profile_QualificationDocument_UploadedAtUtc",
                table: "Applicants");

            migrationBuilder.DropColumn(
                name: "Profile_ReappointmentCondition",
                table: "Applicants");

            migrationBuilder.DropColumn(
                name: "Profile_ReappointmentConditionDetails",
                table: "Applicants");

            migrationBuilder.DropColumn(
                name: "Profile_ReappointmentDepartment",
                table: "Applicants");

            migrationBuilder.DropColumn(
                name: "Profile_ReferenceNumber",
                table: "Applicants");

            migrationBuilder.DropColumn(
                name: "Profile_ResignedPendingDisciplinary",
                table: "Applicants");

            migrationBuilder.DropColumn(
                name: "Profile_ResignedPendingDisciplinaryDetails",
                table: "Applicants");

            migrationBuilder.DropColumn(
                name: "Profile_SignatureData",
                table: "Applicants");

            migrationBuilder.DropColumn(
                name: "Profile_WillRelinquishBusiness",
                table: "Applicants");

            migrationBuilder.DropColumn(
                name: "Profile_WorkPermitDetails",
                table: "Applicants");

            migrationBuilder.AddColumn<string>(
                name: "Profile_Skills",
                table: "Applicants",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Profile_WorkHistory",
                table: "Applicants",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
