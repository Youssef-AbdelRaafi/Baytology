using Baytology.Application.Common.Interfaces;
using Baytology.Domain.Common;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Baytology.Infrastructure.Interceptors;

public class AuditableEntityInterceptor(IUser user) : SaveChangesInterceptor
{
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not null)
        {
            UpdateAuditableEntities(eventData.Context);
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void UpdateAuditableEntities(DbContext context)
    {
        var utcNow = DateTimeOffset.UtcNow;
        var currentUserId = user.Id;

        foreach (var entry in context.ChangeTracker.Entries<AuditableEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedOnUtc = utcNow;
                entry.Entity.CreatedBy = currentUserId;
                entry.Entity.UpdatedOnUtc = utcNow;
                entry.Entity.UpdatedBy = currentUserId;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedOnUtc = utcNow;
                entry.Entity.UpdatedBy = currentUserId;
            }
        }
    }
}
