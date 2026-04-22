using Baytology.Infrastructure.Data;

using Microsoft.EntityFrameworkCore;

namespace Baytology.Api.Tests.Infrastructure;

public sealed class MigrationsParityTests
{
    [Fact]
    public void Entity_framework_model_is_in_sync_with_the_latest_migration_snapshot()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=Baytology_ModelParityProbe;Trusted_Connection=True;")
            .Options;

        using var context = new AppDbContext(options);

        Assert.False(context.Database.HasPendingModelChanges());
    }
}
