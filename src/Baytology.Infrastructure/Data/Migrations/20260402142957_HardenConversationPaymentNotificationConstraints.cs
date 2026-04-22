using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Baytology.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class HardenConversationPaymentNotificationConstraints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PaymentTransactions_PaymentId_GatewayReference_TransactionStatus",
                table: "PaymentTransactions");

            migrationBuilder.AlterColumn<string>(
                name: "RequestedBy",
                table: "RefundRequests",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "TransactionStatus",
                table: "PaymentTransactions",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "GatewayName",
                table: "PaymentTransactions",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_RefundRequests_ReviewState",
                table: "RefundRequests",
                sql: "([Status] = 'Pending' AND [ReviewedBy] IS NULL AND [ReviewedOnUtc] IS NULL) OR ([Status] <> 'Pending' AND [ReviewedBy] IS NOT NULL AND [ReviewedOnUtc] IS NOT NULL)");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentTransactions_PaymentId_GatewayReference_TransactionStatus",
                table: "PaymentTransactions",
                columns: new[] { "PaymentId", "GatewayReference", "TransactionStatus" },
                unique: true,
                filter: "[GatewayReference] IS NOT NULL");

            migrationBuilder.AddCheckConstraint(
                name: "CK_PaymentTransactions_RequiredState",
                table: "PaymentTransactions",
                sql: "LEN(LTRIM(RTRIM([GatewayName]))) > 0 AND LEN(LTRIM(RTRIM([TransactionStatus]))) > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Messages_ContentOrAttachment",
                table: "Messages",
                sql: "LEN([Content]) > 0 OR [AttachmentUrl] IS NOT NULL");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Messages_ReadState",
                table: "Messages",
                sql: "([IsRead] = 0 AND [ReadAt] IS NULL) OR ([IsRead] = 1 AND [ReadAt] IS NOT NULL)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_RefundRequests_ReviewState",
                table: "RefundRequests");

            migrationBuilder.DropIndex(
                name: "IX_PaymentTransactions_PaymentId_GatewayReference_TransactionStatus",
                table: "PaymentTransactions");

            migrationBuilder.DropCheckConstraint(
                name: "CK_PaymentTransactions_RequiredState",
                table: "PaymentTransactions");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Messages_ContentOrAttachment",
                table: "Messages");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Messages_ReadState",
                table: "Messages");

            migrationBuilder.AlterColumn<string>(
                name: "RequestedBy",
                table: "RefundRequests",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldMaxLength: 450);

            migrationBuilder.AlterColumn<string>(
                name: "TransactionStatus",
                table: "PaymentTransactions",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "GatewayName",
                table: "PaymentTransactions",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.CreateIndex(
                name: "IX_PaymentTransactions_PaymentId_GatewayReference_TransactionStatus",
                table: "PaymentTransactions",
                columns: new[] { "PaymentId", "GatewayReference", "TransactionStatus" },
                unique: true,
                filter: "[GatewayReference] IS NOT NULL AND [TransactionStatus] IS NOT NULL");
        }
    }
}
