using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PostCare.infra.Migrations
{
    /// <inheritdoc />
    public partial class SeedFeedingReferences : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 0-6 months - Breastfeeding (FeedingTypeForBaby = 0)
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM FeedingReferences WHERE FeedingTypeForBaby = 0 AND AgeMinMonths = 0 AND AgeMaxMonths = 6)
                BEGIN
                    INSERT INTO FeedingReferences (FeedingTypeForBaby, AgeMinMonths, AgeMaxMonths, MinTimesPerDay, MaxTimesPerDay, Notes)
                    VALUES (0, 0, 6, 8, 12, N'Breastfed: Every 2-3 hours (8-12 times/day)')
                END
            ");

            // 0-6 months - Formula (FeedingTypeForBaby = 1)
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM FeedingReferences WHERE FeedingTypeForBaby = 1 AND AgeMinMonths = 0 AND AgeMaxMonths = 6)
                BEGIN
                    INSERT INTO FeedingReferences (FeedingTypeForBaby, AgeMinMonths, AgeMaxMonths, MinTimesPerDay, MaxTimesPerDay, Notes)
                    VALUES (1, 0, 6, 6, 8, N'Formula-fed: Every 3-4 hours (6-8 times/day)')
                END
            ");

            // 6-12 months - Breastfeeding
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM FeedingReferences WHERE FeedingTypeForBaby = 0 AND AgeMinMonths = 6 AND AgeMaxMonths = 12)
                BEGIN
                    INSERT INTO FeedingReferences (FeedingTypeForBaby, AgeMinMonths, AgeMaxMonths, MinTimesPerDay, MaxTimesPerDay, Notes)
                    VALUES (0, 6, 12, 5, 6, N'Breastfeeding: Every 4-5 hours (5-6 times/day)')
                END
            ");

            // 6-12 months - Formula
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM FeedingReferences WHERE FeedingTypeForBaby = 1 AND AgeMinMonths = 6 AND AgeMaxMonths = 12)
                BEGIN
                    INSERT INTO FeedingReferences (FeedingTypeForBaby, AgeMinMonths, AgeMaxMonths, MinTimesPerDay, MaxTimesPerDay, Notes)
                    VALUES (1, 6, 12, 5, 6, N'Formula: Every 4-5 hours (5-6 times/day)')
                END
            ");

            // 6-12 months - SolidFood (FeedingTypeForBaby = 2)
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM FeedingReferences WHERE FeedingTypeForBaby = 2 AND AgeMinMonths = 6 AND AgeMaxMonths = 12)
                BEGIN
                    INSERT INTO FeedingReferences (FeedingTypeForBaby, AgeMinMonths, AgeMaxMonths, MinTimesPerDay, MaxTimesPerDay, Notes)
                    VALUES (2, 6, 12, 1, 3, N'Solids: 1-2 meals at 6 months, increasing to 3 meals by 12 months')
                END
            ");

            // 12-24 months - Breastfeeding
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM FeedingReferences WHERE FeedingTypeForBaby = 0 AND AgeMinMonths = 12 AND AgeMaxMonths = 24)
                BEGIN
                    INSERT INTO FeedingReferences (FeedingTypeForBaby, AgeMinMonths, AgeMaxMonths, MinTimesPerDay, MaxTimesPerDay, Notes)
                    VALUES (0, 12, 24, 2, 3, N'Breastfeeding: 2-3 times/day')
                END
            ");

            // 12-24 months - Formula
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM FeedingReferences WHERE FeedingTypeForBaby = 1 AND AgeMinMonths = 12 AND AgeMaxMonths = 24)
                BEGIN
                    INSERT INTO FeedingReferences (FeedingTypeForBaby, AgeMinMonths, AgeMaxMonths, MinTimesPerDay, MaxTimesPerDay, Notes)
                    VALUES (1, 12, 24, 2, 3, N'Formula/Milk: 2-3 times/day')
                END
            ");

            // 12-24 months - SolidFood
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM FeedingReferences WHERE FeedingTypeForBaby = 2 AND AgeMinMonths = 12 AND AgeMaxMonths = 24)
                BEGIN
                    INSERT INTO FeedingReferences (FeedingTypeForBaby, AgeMinMonths, AgeMaxMonths, MinTimesPerDay, MaxTimesPerDay, Notes)
                    VALUES (2, 12, 24, 3, 5, N'Solids: 3 meals + 2 snacks daily')
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Delete seeded data when rolling back
            migrationBuilder.Sql(@"
                DELETE FROM FeedingReferences 
                WHERE (FeedingTypeForBaby = 0 AND AgeMinMonths = 0 AND AgeMaxMonths = 6)
                   OR (FeedingTypeForBaby = 1 AND AgeMinMonths = 0 AND AgeMaxMonths = 6)
                   OR (FeedingTypeForBaby = 0 AND AgeMinMonths = 6 AND AgeMaxMonths = 12)
                   OR (FeedingTypeForBaby = 1 AND AgeMinMonths = 6 AND AgeMaxMonths = 12)
                   OR (FeedingTypeForBaby = 2 AND AgeMinMonths = 6 AND AgeMaxMonths = 12)
                   OR (FeedingTypeForBaby = 0 AND AgeMinMonths = 12 AND AgeMaxMonths = 24)
                   OR (FeedingTypeForBaby = 1 AND AgeMinMonths = 12 AND AgeMaxMonths = 24)
                   OR (FeedingTypeForBaby = 2 AND AgeMinMonths = 12 AND AgeMaxMonths = 24)
            ");
        }
    }
}