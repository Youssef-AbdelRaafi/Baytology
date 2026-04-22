using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Baytology.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class HardenRemainingDomainIntegrity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SearchResults_SearchRequestId",
                table: "SearchResults");

            migrationBuilder.DropIndex(
                name: "IX_RecommendationResults_RequestId",
                table: "RecommendationResults");

            migrationBuilder.DropIndex(
                name: "IX_PropertyImages_PropertyId",
                table: "PropertyImages");

            migrationBuilder.CreateIndex(
                name: "IX_SearchResults_SearchRequestId_Rank",
                table: "SearchResults",
                columns: new[] { "SearchRequestId", "Rank" },
                unique: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_SearchResults_BusinessRules",
                table: "SearchResults",
                sql: "[Rank] > 0 AND [RelevanceScore] >= 0 AND ([SnapshotPrice] IS NULL OR [SnapshotPrice] >= 0)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_SearchRequests_State",
                table: "SearchRequests",
                sql: "LEN(LTRIM(RTRIM([UserId]))) > 0 AND [ResultCount] >= 0 AND (([Status] = 'Pending' AND [ResolvedAt] IS NULL) OR ([Status] <> 'Pending' AND [ResolvedAt] IS NOT NULL))");

            migrationBuilder.AddCheckConstraint(
                name: "CK_SearchFilters_Ranges",
                table: "SearchFilters",
                sql: "([MinPrice] IS NULL OR [MinPrice] >= 0) AND ([MaxPrice] IS NULL OR [MaxPrice] >= 0) AND ([MinPrice] IS NULL OR [MaxPrice] IS NULL OR [MinPrice] <= [MaxPrice]) AND ([MinArea] IS NULL OR [MinArea] >= 0) AND ([MaxArea] IS NULL OR [MaxArea] >= 0) AND ([MinArea] IS NULL OR [MaxArea] IS NULL OR [MinArea] <= [MaxArea]) AND ([MinBedrooms] IS NULL OR [MinBedrooms] >= 0) AND ([MaxBedrooms] IS NULL OR [MaxBedrooms] >= 0) AND ([MinBedrooms] IS NULL OR [MaxBedrooms] IS NULL OR [MinBedrooms] <= [MaxBedrooms])");

            migrationBuilder.CreateIndex(
                name: "IX_RecommendationResults_RequestId_Rank",
                table: "RecommendationResults",
                columns: new[] { "RequestId", "Rank" },
                unique: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_RecommendationResults_BusinessRules",
                table: "RecommendationResults",
                sql: "[Rank] > 0 AND [SimilarityScore] >= 0 AND ([SnapshotPrice] IS NULL OR [SnapshotPrice] >= 0) AND ([RecommendedPropertyId] IS NOT NULL OR [ExternalReference] IS NOT NULL)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_RecommendationRequests_BusinessRules",
                table: "RecommendationRequests",
                sql: "LEN(LTRIM(RTRIM([RequestedByUserId]))) > 0 AND LEN(LTRIM(RTRIM([SourceEntityType]))) > 0 AND [TopN] >= 1 AND [TopN] <= 50 AND (([Status] = 'Pending' AND [ResolvedAt] IS NULL) OR ([Status] <> 'Pending' AND [ResolvedAt] IS NOT NULL))");

            migrationBuilder.CreateIndex(
                name: "IX_PropertyImages_PropertyId_SortOrder",
                table: "PropertyImages",
                columns: new[] { "PropertyId", "SortOrder" },
                unique: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_PropertyImages_BusinessRules",
                table: "PropertyImages",
                sql: "LEN(LTRIM(RTRIM([Url]))) > 0 AND [SortOrder] >= 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SearchResults_SearchRequestId_Rank",
                table: "SearchResults");

            migrationBuilder.DropCheckConstraint(
                name: "CK_SearchResults_BusinessRules",
                table: "SearchResults");

            migrationBuilder.DropCheckConstraint(
                name: "CK_SearchRequests_State",
                table: "SearchRequests");

            migrationBuilder.DropCheckConstraint(
                name: "CK_SearchFilters_Ranges",
                table: "SearchFilters");

            migrationBuilder.DropIndex(
                name: "IX_RecommendationResults_RequestId_Rank",
                table: "RecommendationResults");

            migrationBuilder.DropCheckConstraint(
                name: "CK_RecommendationResults_BusinessRules",
                table: "RecommendationResults");

            migrationBuilder.DropCheckConstraint(
                name: "CK_RecommendationRequests_BusinessRules",
                table: "RecommendationRequests");

            migrationBuilder.DropIndex(
                name: "IX_PropertyImages_PropertyId_SortOrder",
                table: "PropertyImages");

            migrationBuilder.DropCheckConstraint(
                name: "CK_PropertyImages_BusinessRules",
                table: "PropertyImages");

            migrationBuilder.CreateIndex(
                name: "IX_SearchResults_SearchRequestId",
                table: "SearchResults",
                column: "SearchRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_RecommendationResults_RequestId",
                table: "RecommendationResults",
                column: "RequestId");

            migrationBuilder.CreateIndex(
                name: "IX_PropertyImages_PropertyId",
                table: "PropertyImages",
                column: "PropertyId");
        }
    }
}
