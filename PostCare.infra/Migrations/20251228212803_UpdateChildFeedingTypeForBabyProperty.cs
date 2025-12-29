using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PostCare.infra.Migrations
{
    /// <inheritdoc />
    public partial class UpdateChildFeedingTypeForBabyProperty : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "FeedingType",
                table: "Children",
                newName: "FeedingTypeForBaby");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "FeedingTypeForBaby",
                table: "Children",
                newName: "FeedingType");
        }
    }
}
