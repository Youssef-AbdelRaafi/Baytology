using Baytology.Domain.Conversations;
using Baytology.Domain.Properties;
using Baytology.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Baytology.Infrastructure.Data.Configurations;

public class ConversationConfiguration : IEntityTypeConfiguration<Conversation>
{
    public void Configure(EntityTypeBuilder<Conversation> builder)
    {
        builder.ToTable("Conversations", table =>
        {
            table.HasCheckConstraint("CK_Conversations_DistinctParticipants", "[BuyerUserId] <> [AgentUserId]");
        });
        builder.HasKey(x => x.Id).IsClustered(false);
        builder.HasOne<Property>().WithMany().HasForeignKey(x => x.PropertyId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<AppUser>().WithMany().HasForeignKey(x => x.BuyerUserId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<AppUser>().WithMany().HasForeignKey(x => x.AgentUserId).OnDelete(DeleteBehavior.Restrict);
        builder.HasMany(x => x.Messages).WithOne().HasForeignKey(x => x.ConversationId).OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(x => x.Messages).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.HasIndex(x => new { x.PropertyId, x.BuyerUserId, x.AgentUserId }).IsUnique();
    }
}

public class MessageConfiguration : IEntityTypeConfiguration<Message>
{
    public void Configure(EntityTypeBuilder<Message> builder)
    {
        builder.ToTable("Messages", table =>
        {
            table.HasCheckConstraint(
                "CK_Messages_ReadState",
                "([IsRead] = 0 AND [ReadAt] IS NULL) OR ([IsRead] = 1 AND [ReadAt] IS NOT NULL)");
            table.HasCheckConstraint(
                "CK_Messages_ContentOrAttachment",
                "LEN([Content]) > 0 OR [AttachmentUrl] IS NOT NULL");
        });
        builder.HasKey(x => x.Id).IsClustered(false);
        builder.Property(x => x.Content).HasMaxLength(5000).IsRequired();
        builder.Property(x => x.AttachmentUrl).HasMaxLength(1000);
        builder.Property(x => x.SenderId).HasMaxLength(450).IsRequired();
        builder.HasOne<AppUser>().WithMany().HasForeignKey(x => x.SenderId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => x.ConversationId);
        builder.Ignore(x => x.DomainEvents);
    }
}
