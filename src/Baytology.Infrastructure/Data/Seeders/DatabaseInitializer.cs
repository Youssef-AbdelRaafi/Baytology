using Baytology.Domain.Common.Enums;
using Baytology.Domain.AgentDetails;
using Baytology.Domain.Identity;
using Baytology.Infrastructure.Identity;
using Baytology.Infrastructure.Settings;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Baytology.Infrastructure.Data.Seeders;

public static class DatabaseInitializer
{
    public static async Task InitialiseDatabaseAsync(this IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<AppDbContext>>();
        var startupSettings = scope.ServiceProvider.GetRequiredService<IOptions<StartupInitializationSettings>>().Value;

        if (!startupSettings.Enabled)
        {
            logger.LogInformation("Startup initialization is disabled by configuration.");
            return;
        }

        try
        {
            if (context.Database.IsRelational())
            {
                logger.LogInformation("Applying migrations...");
                await context.Database.MigrateAsync();
                logger.LogInformation("Migrations applied successfully.");
            }
            else
            {
                logger.LogInformation("Ensuring non-relational database is created...");
                await context.Database.EnsureCreatedAsync();
                logger.LogInformation("Non-relational database created successfully.");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while migrating the database.");
            throw;
        }

        await SeedRolesAsync(scope.ServiceProvider, logger);
        await SeedAdminUserAsync(scope.ServiceProvider, logger);

        if (startupSettings.SeedPropertyCsvOnStartup)
        {
            await PropertyCsvSeeder.SeedAsync(scope.ServiceProvider, logger);
        }
        else
        {
            logger.LogInformation("Property CSV seeding is disabled for this environment.");
        }

        await NormalizeAgentIdentityAsync(scope.ServiceProvider, logger);
    }

    private static async Task SeedRolesAsync(IServiceProvider services, ILogger logger)
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

        foreach (var role in Role.All)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
                logger.LogInformation("Created role: {Role}", role);
            }
        }
    }

    private static async Task SeedAdminUserAsync(IServiceProvider services, ILogger logger)
    {
        var userManager = services.GetRequiredService<UserManager<AppUser>>();
        var adminSettings = services.GetRequiredService<IOptions<AdminSettings>>().Value;
        var adminEmail = adminSettings.DefaultEmail.Trim();
        var adminPassword = adminSettings.DefaultPassword;

        if (string.IsNullOrWhiteSpace(adminEmail) || string.IsNullOrWhiteSpace(adminPassword))
        {
            logger.LogWarning("Admin seeding skipped because AdminSettings:DefaultEmail or AdminSettings:DefaultPassword is not configured.");
            return;
        }

        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser is null)
        {
            adminUser = new AppUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(adminUser, adminPassword);

            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, Role.Admin);
                logger.LogInformation("Admin user created: {Email}", adminEmail);
            }
            else
            {
                logger.LogError("Failed to create admin user: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }
        else
        {
            if (!adminUser.EmailConfirmed)
            {
                adminUser.EmailConfirmed = true;
                await userManager.UpdateAsync(adminUser);
            }

            if (!await userManager.CheckPasswordAsync(adminUser, adminPassword))
            {
                var hasPassword = await userManager.HasPasswordAsync(adminUser);

                IdentityResult passwordResult = hasPassword
                    ? await ResetPasswordAsync(userManager, adminUser, adminPassword)
                    : await userManager.AddPasswordAsync(adminUser, adminPassword);

                if (passwordResult.Succeeded)
                {
                    logger.LogInformation("Existing admin user password was synchronized from configuration.");
                }
                else
                {
                    logger.LogError("Failed to synchronize admin user password: {Errors}", string.Join(", ", passwordResult.Errors.Select(e => e.Description)));
                }
            }

            if (!await userManager.IsInRoleAsync(adminUser, Role.Admin))
            {
                await userManager.AddToRoleAsync(adminUser, Role.Admin);
                logger.LogInformation("Existing admin user was added to role: {Role}", Role.Admin);
            }
        }
    }

    private static async Task<IdentityResult> ResetPasswordAsync(UserManager<AppUser> userManager, AppUser user, string newPassword)
    {
        var resetToken = await userManager.GeneratePasswordResetTokenAsync(user);
        return await userManager.ResetPasswordAsync(user, resetToken, newPassword);
    }

    private static async Task NormalizeAgentIdentityAsync(IServiceProvider services, ILogger logger)
    {
        var context = services.GetRequiredService<AppDbContext>();
        var userManager = services.GetRequiredService<UserManager<AppUser>>();

        var usersWithActiveAgentResponsibilities = await context.Properties
            .Select(property => property.AgentUserId)
            .Union(context.Bookings
                .Where(booking => booking.Status != BookingStatus.Cancelled)
                .Select(booking => booking.AgentUserId))
            .Union(context.Conversations.Select(conversation => conversation.AgentUserId))
            .Distinct()
            .ToListAsync();

        var agentRoleUserIds = await context.UserRoles
            .Join(
                context.Roles.Where(role => role.Name == Role.Agent),
                userRole => userRole.RoleId,
                role => role.Id,
                (userRole, _) => userRole.UserId)
            .Distinct()
            .ToListAsync();

        var agentRoleUserIdSet = agentRoleUserIds.ToHashSet(StringComparer.Ordinal);
        var activeAgentResponsibilitySet = usersWithActiveAgentResponsibilities.ToHashSet(StringComparer.Ordinal);

        foreach (var userId in usersWithActiveAgentResponsibilities.Where(userId => !agentRoleUserIdSet.Contains(userId)))
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user is null)
            {
                logger.LogWarning("Skipped agent-role normalization for missing user {UserId}.", userId);
                continue;
            }

            var addRoleResult = await userManager.AddToRoleAsync(user, Role.Agent);
            if (!addRoleResult.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Failed to normalize Agent role for user '{userId}': {string.Join(", ", addRoleResult.Errors.Select(error => error.Description))}");
            }

            agentRoleUserIdSet.Add(userId);
            logger.LogInformation("Restored Agent role for user {UserId} because they still own active agent records.", userId);
        }

        var usersRequiringAgentDetail = agentRoleUserIdSet
            .Union(activeAgentResponsibilitySet, StringComparer.Ordinal)
            .ToArray();

        var existingAgentDetailUserIds = await context.AgentDetails
            .Select(agentDetail => agentDetail.UserId)
            .ToListAsync();

        var existingAgentDetailSet = existingAgentDetailUserIds.ToHashSet(StringComparer.Ordinal);

        foreach (var userId in usersRequiringAgentDetail.Where(userId => !existingAgentDetailSet.Contains(userId)))
        {
            var agentDetailResult = AgentDetail.Create(userId);
            if (agentDetailResult.IsError)
            {
                throw new InvalidOperationException($"Failed to normalize AgentDetail for user '{userId}'.");
            }

            context.AgentDetails.Add(agentDetailResult.Value);
            existingAgentDetailSet.Add(userId);
        }

        var staleAgentDetails = await context.AgentDetails
            .Where(agentDetail =>
                !agentRoleUserIdSet.Contains(agentDetail.UserId) &&
                !activeAgentResponsibilitySet.Contains(agentDetail.UserId))
            .ToListAsync();

        if (staleAgentDetails.Count > 0)
        {
            context.AgentDetails.RemoveRange(staleAgentDetails);
        }

        if (context.ChangeTracker.HasChanges())
        {
            await context.SaveChangesAsync();
        }

        if (staleAgentDetails.Count > 0)
        {
            logger.LogInformation("Removed {Count} stale agent detail records during startup normalization.", staleAgentDetails.Count);
        }
    }
}
