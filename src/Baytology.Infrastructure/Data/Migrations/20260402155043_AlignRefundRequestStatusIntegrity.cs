using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Baytology.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AlignRefundRequestStatusIntegrity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_RefundRequests_ReviewState",
                table: "RefundRequests");

            migrationBuilder.AddCheckConstraint(
                name: "CK_RefundRequests_ReviewState",
                table: "RefundRequests",
                sql: "([Status] = 'Pending' AND [ReviewedBy] IS NULL AND [ReviewedOnUtc] IS NULL) OR ([Status] IN ('Approved','Rejected','Processed') AND [ReviewedBy] IS NOT NULL AND [ReviewedOnUtc] IS NOT NULL)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_RefundRequests_Status_Valid",
                table: "RefundRequests",
                sql: "[Status] IN ('Pending','Approved','Rejected','Processed')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_RefundRequests_ReviewState",
                table: "RefundRequests");

            migrationBuilder.DropCheckConstraint(
                name: "CK_RefundRequests_Status_Valid",
                table: "RefundRequests");

            migrationBuilder.AddCheckConstraint(
                name: "CK_RefundRequests_ReviewState",
                table: "RefundRequests",
                sql: "([Status] = 'Pending' AND [ReviewedBy] IS NULL AND [ReviewedOnUtc] IS NULL) OR ([Status] <> 'Pending' AND [ReviewedBy] IS NOT NULL AND [ReviewedOnUtc] IS NOT NULL)");
        }
    }
}
