using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CLTI.Diagnosis.Migrations
{
    /// <inheritdoc />
    public partial class AddCaseHomeMetadata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CaseStatus",
                table: "u_clti",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Open");

            migrationBuilder.AddColumn<bool>(
                name: "Is2YLECompleted",
                table: "u_clti",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsCRABCompleted",
                table: "u_clti",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsGLASSCompleted",
                table: "u_clti",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsGLASSFemoroPoplitealCompleted",
                table: "u_clti",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsGLASSFinalCompleted",
                table: "u_clti",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsGLASSInfrapoplitealCompleted",
                table: "u_clti",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsICompleted",
                table: "u_clti",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsRevascularizationAssessmentCompleted",
                table: "u_clti",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsRevascularizationMethodCompleted",
                table: "u_clti",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsSubmalleolarDiseaseCompleted",
                table: "u_clti",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsSurgicalRiskCompleted",
                table: "u_clti",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsWCompleted",
                table: "u_clti",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsWiFIResultsCompleted",
                table: "u_clti",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsfICompleted",
                table: "u_clti",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "LastClosedStep",
                table: "u_clti",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastVisitedStep",
                table: "u_clti",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PatientFullName",
                table: "u_clti",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "Без імені");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CaseStatus",
                table: "u_clti");

            migrationBuilder.DropColumn(
                name: "Is2YLECompleted",
                table: "u_clti");

            migrationBuilder.DropColumn(
                name: "IsCRABCompleted",
                table: "u_clti");

            migrationBuilder.DropColumn(
                name: "IsGLASSCompleted",
                table: "u_clti");

            migrationBuilder.DropColumn(
                name: "IsGLASSFemoroPoplitealCompleted",
                table: "u_clti");

            migrationBuilder.DropColumn(
                name: "IsGLASSFinalCompleted",
                table: "u_clti");

            migrationBuilder.DropColumn(
                name: "IsGLASSInfrapoplitealCompleted",
                table: "u_clti");

            migrationBuilder.DropColumn(
                name: "IsICompleted",
                table: "u_clti");

            migrationBuilder.DropColumn(
                name: "IsRevascularizationAssessmentCompleted",
                table: "u_clti");

            migrationBuilder.DropColumn(
                name: "IsRevascularizationMethodCompleted",
                table: "u_clti");

            migrationBuilder.DropColumn(
                name: "IsSubmalleolarDiseaseCompleted",
                table: "u_clti");

            migrationBuilder.DropColumn(
                name: "IsSurgicalRiskCompleted",
                table: "u_clti");

            migrationBuilder.DropColumn(
                name: "IsWCompleted",
                table: "u_clti");

            migrationBuilder.DropColumn(
                name: "IsWiFIResultsCompleted",
                table: "u_clti");

            migrationBuilder.DropColumn(
                name: "IsfICompleted",
                table: "u_clti");

            migrationBuilder.DropColumn(
                name: "LastClosedStep",
                table: "u_clti");

            migrationBuilder.DropColumn(
                name: "LastVisitedStep",
                table: "u_clti");

            migrationBuilder.DropColumn(
                name: "PatientFullName",
                table: "u_clti");
        }
    }
}
