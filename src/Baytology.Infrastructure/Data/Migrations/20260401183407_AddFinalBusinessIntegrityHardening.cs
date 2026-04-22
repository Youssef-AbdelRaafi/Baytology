using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Baytology.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddFinalBusinessIntegrityHardening : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddCheckConstraint(
                name: "CK_RefundRequests_Amount_Positive",
                table: "RefundRequests",
                sql: "[Amount] > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Properties_BusinessRules",
                table: "Properties",
                sql: "[Price] > 0 AND [Area] > 0 AND [Bedrooms] >= 0 AND [Bathrooms] >= 0 AND ([Floor] IS NULL OR [Floor] >= 0) AND ([TotalFloors] IS NULL OR [TotalFloors] > 0) AND ([Floor] IS NULL OR [TotalFloors] IS NULL OR [Floor] <= [TotalFloors]) AND ([Latitude] IS NULL OR ([Latitude] >= -90 AND [Latitude] <= 90)) AND ([Longitude] IS NULL OR ([Longitude] >= -180 AND [Longitude] <= 180))");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Payments_Amounts_Valid",
                table: "Payments",
                sql: "[Amount] > 0 AND [Commission] >= 0 AND [Commission] <= [Amount] AND [NetAmount] = [Amount] - [Commission]");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Notifications_ReadState",
                table: "Notifications",
                sql: "([IsRead] = 0 AND [ReadAt] IS NULL) OR ([IsRead] = 1 AND [ReadAt] IS NOT NULL)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Conversations_DistinctParticipants",
                table: "Conversations",
                sql: "[BuyerUserId] <> [AgentUserId]");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Bookings_DateRange",
                table: "Bookings",
                sql: "[StartDate] < [EndDate]");

            migrationBuilder.CreateIndex(
                name: "IX_AgentReviews_AgentUserId_ReviewerUserId",
                table: "AgentReviews",
                columns: new[] { "AgentUserId", "ReviewerUserId" },
                unique: true,
                filter: "[PropertyId] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AgentReviews_AgentUserId_ReviewerUserId_PropertyId",
                table: "AgentReviews",
                columns: new[] { "AgentUserId", "ReviewerUserId", "PropertyId" },
                unique: true,
                filter: "[PropertyId] IS NOT NULL");

            migrationBuilder.AddCheckConstraint(
                name: "CK_AgentReviews_DistinctUsers",
                table: "AgentReviews",
                sql: "[AgentUserId] <> [ReviewerUserId]");

            migrationBuilder.AddCheckConstraint(
                name: "CK_AgentReviews_Rating_Range",
                table: "AgentReviews",
                sql: "[Rating] >= 1 AND [Rating] <= 5");

            migrationBuilder.AddCheckConstraint(
                name: "CK_AgentDetails_CommissionRate_Range",
                table: "AgentDetails",
                sql: "[CommissionRate] > 0 AND [CommissionRate] < 1");

            migrationBuilder.AddCheckConstraint(
                name: "CK_AgentDetails_Rating_Range",
                table: "AgentDetails",
                sql: "[Rating] >= 0 AND [Rating] <= 5");

            migrationBuilder.AddCheckConstraint(
                name: "CK_AgentDetails_ReviewCount_NonNegative",
                table: "AgentDetails",
                sql: "[ReviewCount] >= 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_RefundRequests_Amount_Positive",
                table: "RefundRequests");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Properties_BusinessRules",
                table: "Properties");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Payments_Amounts_Valid",
                table: "Payments");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Notifications_ReadState",
                table: "Notifications");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Conversations_DistinctParticipants",
                table: "Conversations");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Bookings_DateRange",
                table: "Bookings");

            migrationBuilder.DropIndex(
                name: "IX_AgentReviews_AgentUserId_ReviewerUserId",
                table: "AgentReviews");

            migrationBuilder.DropIndex(
                name: "IX_AgentReviews_AgentUserId_ReviewerUserId_PropertyId",
                table: "AgentReviews");

            migrationBuilder.DropCheckConstraint(
                name: "CK_AgentReviews_DistinctUsers",
                table: "AgentReviews");

            migrationBuilder.DropCheckConstraint(
                name: "CK_AgentReviews_Rating_Range",
                table: "AgentReviews");

            migrationBuilder.DropCheckConstraint(
                name: "CK_AgentDetails_CommissionRate_Range",
                table: "AgentDetails");

            migrationBuilder.DropCheckConstraint(
                name: "CK_AgentDetails_Rating_Range",
                table: "AgentDetails");

            migrationBuilder.DropCheckConstraint(
                name: "CK_AgentDetails_ReviewCount_NonNegative",
                table: "AgentDetails");
        }
    }
}
