using System.Text.Json;

using Baytology.Application.Common.Interfaces;
using Baytology.Domain.AuditLogs;
using Baytology.Domain.Common;
using Baytology.Infrastructure.Data;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Baytology.Infrastructure.Interceptors;

public class AuditLogInterceptor(IUser user) : SaveChangesInterceptor
{
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is AppDbContext context)
        {
            CreateAuditLogs(context);
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void CreateAuditLogs(AppDbContext context)
    {
        var entries = context.ChangeTracker
            .Entries<Entity>()
            .Where(e => e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            .Where(e => e.Entity is not AuditLog) // Don't audit audit logs
            .ToList();

        foreach (var entry in entries)
        {
            var action = entry.State switch
            {
                EntityState.Added => "Created",
                EntityState.Modified => "Updated",
                EntityState.Deleted => "Deleted",
                _ => "Unknown"
            };

            string? oldValues = null;
            string? newValues = null;

            if (entry.State == EntityState.Modified)
            {
                var modifiedProps = entry.Properties
                    .Where(p => p.IsModified)
                    .ToDictionary(p => p.Metadata.Name, p => p.OriginalValue);
                oldValues = JsonSerializer.Serialize(modifiedProps);

                var currentProps = entry.Properties
                    .Where(p => p.IsModified)
                    .ToDictionary(p => p.Metadata.Name, p => p.CurrentValue);
                newValues = JsonSerializer.Serialize(currentProps);
            }
            else if (entry.State == EntityState.Added)
            {
                var props = entry.Properties
                    .ToDictionary(p => p.Metadata.Name, p => p.CurrentValue);
                newValues = JsonSerializer.Serialize(props);
            }

            var auditLogResult = AuditLog.Create(
                user.Id,
                action,
                entry.Entity.GetType().Name,
                entry.Entity.Id.ToString(),
                oldValues,
                newValues,
                null);

            if (auditLogResult.IsError)
                throw new InvalidOperationException($"Failed to create audit log for {entry.Entity.GetType().Name}.");

            context.AuditLogs.Add(auditLogResult.Value);
        }
    }
}
