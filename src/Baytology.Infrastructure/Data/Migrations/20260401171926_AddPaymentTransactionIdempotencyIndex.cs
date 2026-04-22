using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Baytology.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentTransactionIdempotencyIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PaymentTransactions_PaymentId",
                table: "PaymentTransactions");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentTransactions_PaymentId_GatewayReference_TransactionStatus",
                table: "PaymentTransactions",
                columns: new[] { "PaymentId", "GatewayReference", "TransactionStatus" },
                unique: true,
                filter: "[GatewayReference] IS NOT NULL AND [TransactionStatus] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PaymentTransactions_PaymentId_GatewayReference_TransactionStatus",
                table: "PaymentTransactions");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentTransactions_PaymentId",
                table: "PaymentTransactions",
                column: "PaymentId");
        }
    }
}
