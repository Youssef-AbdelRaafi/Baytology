using System.Text;

using Baytology.Application.Common.Interfaces;
using Baytology.Infrastructure.AI;
using Baytology.Infrastructure.BackgroundJobs;
using Baytology.Infrastructure.Caching;
using Baytology.Infrastructure.Data;
using Baytology.Infrastructure.Identity;
using Baytology.Infrastructure.Interceptors;
using Baytology.Infrastructure.Messaging;
using Baytology.Infrastructure.Notifications;
using Baytology.Infrastructure.Payments;
using Baytology.Infrastructure.RealTime;
using Baytology.Infrastructure.Settings;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        services
            .AddStartupInitializationServices(configuration)
            .AddCachingServices()
            .AddDatabaseServices(configuration, environment)
            .AddIdentityServices(configuration, environment)
            .AddMessagingServices(configuration)
            .AddPaymentServices(configuration)
            .AddAiFallbackServices(configuration)
            .AddExternalAiIntegrationServices(configuration)
            .AddNotificationServices()
            .AddBackgroundJobs();

        return services;
    }

    private static IServiceCollection AddStartupInitializationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<StartupInitializationSettings>()
            .Bind(configuration.GetSection("StartupInitialization"));

        return services;
    }

    private static IServiceCollection AddCachingServices(this IServiceCollection services)
    {
        services.AddHybridCache(options =>
        {
            options.MaximumPayloadBytes = 1024 * 1024;
            options.MaximumKeyLength = 1024;
            options.DefaultEntryOptions = new()
            {
                Expiration = TimeSpan.FromMinutes(5),
                LocalCacheExpiration = TimeSpan.FromMinutes(2)
            };
        });

        services.AddScoped<IQueryCache, HybridQueryCache>();

        return services;
    }

    private static IServiceCollection AddDatabaseServices(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        services.AddScoped<ISaveChangesInterceptor, AuditableEntityInterceptor>();
        services.AddScoped<ISaveChangesInterceptor, DomainEventInterceptor>();
        services.AddScoped<ISaveChangesInterceptor, AuditLogInterceptor>();

        var connectionString = configuration.GetConnectionString("DefaultConnection");

        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException("ConnectionStrings:DefaultConnection is required. Configure it via user-secrets or environment variables.");

        var normalizedConnectionString = NormalizeConnectionString(connectionString, environment);

        services.AddDbContext<AppDbContext>((sp, options) =>
        {
            options.AddInterceptors(sp.GetServices<ISaveChangesInterceptor>());
            options.UseSqlServer(
                normalizedConnectionString,
                sqlOptions =>
                {
                    sqlOptions.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName);
                    sqlOptions.EnableRetryOnFailure();
                });
        });

        services.AddScoped<IAppDbContext>(sp => sp.GetRequiredService<AppDbContext>());

        return services;
    }

    private static string NormalizeConnectionString(string connectionString, IHostEnvironment environment)
    {
        var builder = new SqlConnectionStringBuilder(connectionString);

        if (environment.IsDevelopment() && IsLocalSqlServer(builder.DataSource))
        {
            builder.DataSource = NormalizeLocalSqlServerDataSource(builder.DataSource);
            builder["Encrypt"] = "False";
            builder["TrustServerCertificate"] = "True";
        }

        return builder.ConnectionString;
    }

    private static bool IsLocalSqlServer(string? dataSource)
    {
        if (string.IsNullOrWhiteSpace(dataSource))
            return false;

        var normalized = dataSource.Trim();

        return normalized.Equals(".", StringComparison.OrdinalIgnoreCase)
            || normalized.Equals("(local)", StringComparison.OrdinalIgnoreCase)
            || normalized.Equals("localhost", StringComparison.OrdinalIgnoreCase)
            || normalized.Equals("127.0.0.1", StringComparison.OrdinalIgnoreCase)
            || normalized.StartsWith(@".\", StringComparison.OrdinalIgnoreCase)
            || normalized.StartsWith(@"(localdb)\", StringComparison.OrdinalIgnoreCase)
            || normalized.StartsWith(@"localhost\", StringComparison.OrdinalIgnoreCase)
            || normalized.StartsWith(@"127.0.0.1\", StringComparison.OrdinalIgnoreCase);
    }

    private static string NormalizeLocalSqlServerDataSource(string? dataSource)
    {
        if (string.IsNullOrWhiteSpace(dataSource))
            return "localhost";

        var normalized = dataSource.Trim();

        if (normalized.Equals(".", StringComparison.OrdinalIgnoreCase)
            || normalized.Equals("(local)", StringComparison.OrdinalIgnoreCase))
        {
            return "localhost";
        }

        if (normalized.StartsWith(@".\", StringComparison.OrdinalIgnoreCase))
        {
            return $"localhost\\{normalized[2..]}";
        }

        return normalized;
    }

    private static IServiceCollection AddIdentityServices(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        services.AddOptions<JwtSettings>()
            .Bind(configuration.GetSection("JwtSettings"))
            .Validate(settings => !string.IsNullOrWhiteSpace(settings.Secret), "JwtSettings:Secret is required.")
            .Validate(settings => settings.Secret.Length >= 32, "JwtSettings:Secret must be at least 32 characters.")
            .ValidateOnStart();

        var emailOptions = services.AddOptions<EmailSettings>()
            .Bind(configuration.GetSection("Email"))
            .Validate(settings => !string.IsNullOrWhiteSpace(settings.FromAddress), "Email:FromAddress is required.")
            .Validate(settings => settings.DeliveryMode != EmailDeliveryMode.Smtp || !string.IsNullOrWhiteSpace(settings.SmtpHost),
                "Email:SmtpHost is required when DeliveryMode is Smtp.")
            .Validate(settings => settings.DeliveryMode != EmailDeliveryMode.Smtp || settings.SmtpPort > 0,
                "Email:SmtpPort must be greater than zero when DeliveryMode is Smtp.");

        if (!environment.IsDevelopment() && !environment.IsEnvironment("Testing"))
        {
            emailOptions.Validate(
                settings => settings.DeliveryMode == EmailDeliveryMode.Smtp,
                "Email:DeliveryMode must be Smtp outside Development.");
        }

        emailOptions.ValidateOnStart();

        services.AddOptions<AdminSettings>()
            .Bind(configuration.GetSection("AdminSettings"));

        services.AddIdentity<AppUser, IdentityRole>(options =>
        {
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireNonAlphanumeric = true;
            options.Password.RequiredLength = 8;
            options.User.RequireUniqueEmail = true;
        })
        .AddEntityFrameworkStores<AppDbContext>()
        .AddDefaultTokenProviders();

        var jwtSettings = configuration.GetSection("JwtSettings").Get<JwtSettings>()
            ?? throw new InvalidOperationException("JwtSettings configuration is missing.");

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret)),
                ValidateIssuer = true,
                ValidIssuer = jwtSettings.Issuer,
                ValidateAudience = true,
                ValidAudience = jwtSettings.Audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            // Support SignalR token via query string
            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    var accessToken = context.Request.Query["access_token"];
                    var path = context.HttpContext.Request.Path;
                    if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                    {
                        context.Token = accessToken;
                    }
                    return Task.CompletedTask;
                }
            };
        });

        services.AddAuthorizationBuilder();

        services.AddScoped<IIdentityService, IdentityService>();
        services.AddScoped<ITokenProvider, TokenProvider>();

        // External Login & Notifications
        services.AddOptions<GoogleAuthSettings>().Bind(configuration.GetSection("GoogleAuthSettings"));
        services.AddOptions<FacebookAuthSettings>().Bind(configuration.GetSection("FacebookAuthSettings"));
        services.AddScoped<IExternalLoginTokenValidator, ExternalLoginTokenValidator>();
        services.AddScoped<IEmailSender, ConfiguredEmailSender>();

        return services;
    }

    private static IServiceCollection AddMessagingServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<RabbitMqSettings>()
            .Bind(configuration.GetSection("RabbitMq"))
            .Validate(settings => !settings.Enabled || !string.IsNullOrWhiteSpace(settings.HostName),
                "RabbitMq:HostName is required when RabbitMQ is enabled.")
            .Validate(settings => !settings.Enabled || settings.Port > 0,
                "RabbitMq:Port must be greater than zero when RabbitMQ is enabled.")
            .Validate(settings => !settings.Enabled || !string.IsNullOrWhiteSpace(settings.SearchQueueName),
                "RabbitMq:SearchQueueName is required when RabbitMQ is enabled.")
            .Validate(settings => !settings.Enabled || !string.IsNullOrWhiteSpace(settings.RecommendationQueueName),
                "RabbitMq:RecommendationQueueName is required when RabbitMQ is enabled.")
            .Validate(settings => !settings.Enabled || !string.IsNullOrWhiteSpace(settings.PropertyIndexQueueName),
                "RabbitMq:PropertyIndexQueueName is required when RabbitMQ is enabled.")
            .Validate(settings => !settings.Enabled || !string.IsNullOrWhiteSpace(settings.UserHistoryQueueName),
                "RabbitMq:UserHistoryQueueName is required when RabbitMQ is enabled.")
            .ValidateOnStart();
        services.AddSingleton<IMessagePublisher, RabbitMqPublisher>();

        return services;
    }

    private static IServiceCollection AddPaymentServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<PaymobSettings>()
            .Bind(configuration.GetSection("Paymob"));
        services.AddHttpClient<IPaymentGateway, PaymobGateway>(client =>
            client.Timeout = TimeSpan.FromSeconds(30))
            .AddStandardResilienceHandler();

        return services;
    }

    private static IServiceCollection AddNotificationServices(this IServiceCollection services)
    {
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IConversationRealtimeService, ConversationRealtimeService>();
        return services;
    }

    private static IServiceCollection AddAiFallbackServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<AiProcessingSettings>()
            .Bind(configuration.GetSection("AiProcessing"));

        services.AddScoped<IAiDispatchPolicy, RabbitMqAiDispatchPolicy>();
        services.AddScoped<IAiSearchFallbackService, InternalAiSearchFallbackService>();
        services.AddScoped<IRecommendationFallbackService, InternalRecommendationFallbackService>();

        return services;
    }

    private static IServiceCollection AddExternalAiIntegrationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<ExternalAiServicesSettings>()
            .Bind(configuration.GetSection("ExternalAiServices"));
        var externalAiTimeout = TimeSpan.FromSeconds(Math.Max(1, configuration.GetValue<int>("ExternalAiServices:TimeoutSeconds")));

        services.AddHttpClient<IChatbotApiClient, ChatbotApiClient>((sp, client) =>
        {
            var settings = sp.GetRequiredService<IOptions<ExternalAiServicesSettings>>().Value;
            client.Timeout = TimeSpan.FromSeconds(Math.Max(1, settings.TimeoutSeconds));

            if (Uri.TryCreate(settings.ChatbotBaseUrl, UriKind.Absolute, out var baseUri))
                client.BaseAddress = baseUri;
        }).AddStandardResilienceHandler(options =>
        {
            options.AttemptTimeout.Timeout = externalAiTimeout;
            options.TotalRequestTimeout.Timeout = externalAiTimeout;
            options.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(Math.Max(30, externalAiTimeout.TotalSeconds * 2));
        });

        services.AddHttpClient<IRecommendationApiClient, RecommendationApiClient>((sp, client) =>
        {
            var settings = sp.GetRequiredService<IOptions<ExternalAiServicesSettings>>().Value;
            client.Timeout = TimeSpan.FromSeconds(Math.Max(1, settings.TimeoutSeconds));

            if (Uri.TryCreate(settings.RecommendationBaseUrl, UriKind.Absolute, out var baseUri))
                client.BaseAddress = baseUri;
        }).AddStandardResilienceHandler(options =>
        {
            options.AttemptTimeout.Timeout = externalAiTimeout;
            options.TotalRequestTimeout.Timeout = externalAiTimeout;
            options.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(Math.Max(30, externalAiTimeout.TotalSeconds * 2));
        });

        return services;
    }

    private static IServiceCollection AddBackgroundJobs(this IServiceCollection services)
    {
        services.AddHostedService<OutboxProcessor>();
        services.AddHostedService<AiFallbackRecoveryProcessor>();
        return services;
    }
}
