using Baytology.Domain.AgentDetails;
using Baytology.Infrastructure.Identity;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Baytology.Infrastructure.Data.Configurations;

public class AgentDetailConfiguration : IEntityTypeConfiguration<AgentDetail>
{
    public void Configure(EntityTypeBuilder<AgentDetail> builder)
    {
        builder.ToTable("AgentDetails", table =>
        {
            table.HasCheckConstraint(
                "CK_AgentDetails_CommissionRate_Range",
                "[CommissionRate] > 0 AND [CommissionRate] < 1");
            table.HasCheckConstraint(
                "CK_AgentDetails_Rating_Range",
                "[Rating] >= 0 AND [Rating] <= 5");
            table.HasCheckConstraint(
                "CK_AgentDetails_ReviewCount_NonNegative",
                "[ReviewCount] >= 0");
        });
        builder.HasKey(x => x.Id).IsClustered(false);
        builder.HasIndex(x => x.UserId).IsUnique();
        builder.Property(x => x.AgencyName).HasMaxLength(300);
        builder.Property(x => x.LicenseNumber).HasMaxLength(100);
        builder.Property(x => x.Rating).HasPrecision(3, 2);
        builder.Property(x => x.CommissionRate).HasPrecision(5, 4);
        builder.HasOne<AppUser>().WithOne().HasForeignKey<AgentDetail>(x => x.UserId);
    }
}
