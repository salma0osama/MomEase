using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MomEase.infra.Migrations
{
    /// <inheritdoc />
    public partial class AddGoogleAuthFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.RenameColumn(
            //    name: "FeedingType",
            //    table: "Children",
            //    newName: "FeedingTypeForBaby");

            migrationBuilder.AddColumn<string>(
                name: "GoogleId",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsExternalAuth",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GoogleId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "IsExternalAuth",
                table: "Users");

            migrationBuilder.RenameColumn(
                name: "FeedingTypeForBaby",
                table: "Children",
                newName: "FeedingType");
        }
    }
}
