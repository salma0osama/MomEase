using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MomEase.infra.Migrations
{
    /// <inheritdoc />
    public partial class publish : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DiseasePreventedAr",
                table: "Vaccinations",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DosageAr",
                table: "Vaccinations",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DoseTimingAr",
                table: "Vaccinations",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VaccinationWayAr",
                table: "Vaccinations",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VaccineAr",
                table: "Vaccinations",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PreferredLanguage",
                table: "Users",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<double>(
                name: "Confidence",
                table: "SkinAnalyses",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AdviceAr",
                table: "ScoreLevels",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LevelNameAr",
                table: "ScoreLevels",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "QuestionTextAr",
                table: "Questions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AdviceAr",
                table: "Diseases",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NameAr",
                table: "Diseases",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DescriptionAr",
                table: "Assessments",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NameAr",
                table: "Assessments",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContentAr",
                table: "Articles",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TitleAr",
                table: "Articles",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DescriptionAr",
                table: "ArticleCategories",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NameAr",
                table: "ArticleCategories",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OptionTextAr",
                table: "AnswerOptions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "MentalHealthFollowUps",
                columns: table => new
                {
                    FollowUpId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    LastAssessmentResultId = table.Column<int>(type: "int", nullable: false),
                    SeverityLevel = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    NextAssessmentDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NextTipDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AssessmentReminderSent = table.Column<bool>(type: "bit", nullable: false),
                    IsCompleted = table.Column<bool>(type: "bit", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MentalHealthFollowUps", x => x.FollowUpId);
                    table.ForeignKey(
                        name: "FK_MentalHealthFollowUps_AssessmentResults_LastAssessmentResultId",
                        column: x => x.LastAssessmentResultId,
                        principalTable: "AssessmentResults",
                        principalColumn: "ResultId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MentalHealthFollowUps_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MentalHealthTips",
                columns: table => new
                {
                    TipId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SeverityLevel = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SeverityLevelAr = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    TipTextEnglish = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TipTextAr = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Category = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CategoryAr = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MentalHealthTips", x => x.TipId);
                });

            migrationBuilder.CreateTable(
                name: "SentMentalHealthTips",
                columns: table => new
                {
                    SentTipId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    TipId = table.Column<int>(type: "int", nullable: false),
                    SentAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SentMentalHealthTips", x => x.SentTipId);
                    table.ForeignKey(
                        name: "FK_SentMentalHealthTips_MentalHealthTips_TipId",
                        column: x => x.TipId,
                        principalTable: "MentalHealthTips",
                        principalColumn: "TipId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SentMentalHealthTips_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MentalHealthFollowUps_LastAssessmentResultId",
                table: "MentalHealthFollowUps",
                column: "LastAssessmentResultId");

            migrationBuilder.CreateIndex(
                name: "IX_MentalHealthFollowUps_UserId",
                table: "MentalHealthFollowUps",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SentMentalHealthTips_TipId",
                table: "SentMentalHealthTips",
                column: "TipId");

            migrationBuilder.CreateIndex(
                name: "IX_SentMentalHealthTips_UserId",
                table: "SentMentalHealthTips",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MentalHealthFollowUps");

            migrationBuilder.DropTable(
                name: "SentMentalHealthTips");

            migrationBuilder.DropTable(
                name: "MentalHealthTips");

            migrationBuilder.DropColumn(
                name: "DiseasePreventedAr",
                table: "Vaccinations");

            migrationBuilder.DropColumn(
                name: "DosageAr",
                table: "Vaccinations");

            migrationBuilder.DropColumn(
                name: "DoseTimingAr",
                table: "Vaccinations");

            migrationBuilder.DropColumn(
                name: "VaccinationWayAr",
                table: "Vaccinations");

            migrationBuilder.DropColumn(
                name: "VaccineAr",
                table: "Vaccinations");

            migrationBuilder.DropColumn(
                name: "PreferredLanguage",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Confidence",
                table: "SkinAnalyses");

            migrationBuilder.DropColumn(
                name: "AdviceAr",
                table: "ScoreLevels");

            migrationBuilder.DropColumn(
                name: "LevelNameAr",
                table: "ScoreLevels");

            migrationBuilder.DropColumn(
                name: "QuestionTextAr",
                table: "Questions");

            migrationBuilder.DropColumn(
                name: "AdviceAr",
                table: "Diseases");

            migrationBuilder.DropColumn(
                name: "NameAr",
                table: "Diseases");

            migrationBuilder.DropColumn(
                name: "DescriptionAr",
                table: "Assessments");

            migrationBuilder.DropColumn(
                name: "NameAr",
                table: "Assessments");

            migrationBuilder.DropColumn(
                name: "ContentAr",
                table: "Articles");

            migrationBuilder.DropColumn(
                name: "TitleAr",
                table: "Articles");

            migrationBuilder.DropColumn(
                name: "DescriptionAr",
                table: "ArticleCategories");

            migrationBuilder.DropColumn(
                name: "NameAr",
                table: "ArticleCategories");

            migrationBuilder.DropColumn(
                name: "OptionTextAr",
                table: "AnswerOptions");
        }
    }
}
