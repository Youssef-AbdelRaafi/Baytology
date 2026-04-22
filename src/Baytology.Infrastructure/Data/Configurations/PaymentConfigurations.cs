using Baytology.Domain.Payments;
using Baytology.Domain.Properties;
using Baytology.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Baytology.Infrastructure.Data.Configurations;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("Payments", table =>
        {
            table.HasCheckConstraint(
                "CK_Payments_Amounts_Valid",
                "[Amount] > 0 AND [Commission] >= 0 AND [Commission] <= [Amount] AND [NetAmount] = [Amount] - [Commission]");
        });
        builder.HasKey(x => x.Id).IsClustered(false);
        builder.Property(x => x.Amount).HasPrecision(18, 2);
        builder.Property(x => x.Commission).HasPrecision(18, 2);
        builder.Property(x => x.NetAmount).HasPrecision(18, 2);
        builder.Property(x => x.Currency).HasMaxLength(10);
        builder.Property(x => x.Purpose).HasConversion<string>().HasMaxLength(30);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(30);
        builder.Property(x => x.Notes).HasMaxLength(2000);
        builder.HasOne<AppUser>().WithMany().HasForeignKey(x => x.PayerId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<AppUser>().WithMany().HasForeignKey(x => x.PayeeId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Property>().WithMany().HasForeignKey(x => x.PropertyId).OnDelete(DeleteBehavior.Restrict);
        builder.HasMany(x => x.Transactions).WithOne().HasForeignKey(x => x.PaymentId).OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(x => x.Transactions).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Ignore(x => x.DomainEvents);
    }
}

public class PaymentTransactionConfiguration : IEntityTypeConfiguration<PaymentTransaction>
{
    public void Configure(EntityTypeBuilder<PaymentTransaction> builder)
    {
        builder.ToTable("PaymentTransactions", table =>
        {
            table.HasCheckConstraint(
                "CK_PaymentTransactions_RequiredState",
                "LEN(LTRIM(RTRIM([GatewayName]))) > 0 AND LEN(LTRIM(RTRIM([TransactionStatus]))) > 0");
        });
        builder.HasKey(x => x.Id).IsClustered(false);
        builder.Property(x => x.GatewayReference).HasMaxLength(200);
        builder.Property(x => x.GatewayName).HasMaxLength(50).IsRequired();
        builder.Property(x => x.TransactionStatus).HasMaxLength(50).IsRequired();
        builder.Property(x => x.RawResponse).HasColumnType("nvarchar(max)");
        builder.HasIndex(x => new { x.PaymentId, x.GatewayReference, x.TransactionStatus }).IsUnique();
    }
}

public class RefundRequestConfiguration : IEntityTypeConfiguration<RefundRequest>
{
    public void Configure(EntityTypeBuilder<RefundRequest> builder)
    {
        builder.ToTable("RefundRequests", table =>
        {
            table.HasCheckConstraint("CK_RefundRequests_Amount_Positive", "[Amount] > 0");
            table.HasCheckConstraint(
                "CK_RefundRequests_Status_Valid",
                "[Status] IN ('Pending','Approved','Rejected','Processed')");
            table.HasCheckConstraint(
                "CK_RefundRequests_ReviewState",
                "([Status] = 'Pending' AND [ReviewedBy] IS NULL AND [ReviewedOnUtc] IS NULL) OR ([Status] IN ('Approved','Rejected','Processed') AND [ReviewedBy] IS NOT NULL AND [ReviewedOnUtc] IS NOT NULL)");
        });
        builder.HasKey(x => x.Id).IsClustered(false);
        builder.Property(x => x.RequestedBy).HasMaxLength(450).IsRequired();
        builder.Property(x => x.Reason).HasMaxLength(2000).IsRequired();
        builder.Property(x => x.Amount).HasPrecision(18, 2);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(30);
        builder.Property(x => x.ReviewedBy).HasMaxLength(450);
        builder.HasOne<Payment>().WithMany().HasForeignKey(x => x.PaymentId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => new { x.PaymentId, x.Status });
        builder.HasIndex(x => x.PaymentId)
            .IsUnique()
            .HasFilter("[Status] = 'Pending'");
    }
}
