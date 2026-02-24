using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MomEase.infra.Migrations
{
    /// <inheritdoc />
    public partial class articleCategotyImgURL : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "ArticleCategories",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "ArticleCategories");
        }
    }
}
