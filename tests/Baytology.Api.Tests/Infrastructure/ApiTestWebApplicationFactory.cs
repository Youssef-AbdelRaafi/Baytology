using Baytology.Application.Common.Interfaces;
using Baytology.Infrastructure.BackgroundJobs;
using Baytology.Infrastructure.Data;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Baytology.Api.Tests.Infrastructure;

public class ApiTestWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly InMemoryDatabaseRoot _databaseRoot = new();
    private readonly IServiceProvider _inMemoryEntityFrameworkProvider = new ServiceCollection()
        .AddEntityFrameworkInMemoryDatabase()
        .BuildServiceProvider();

    public TestSeedData SeedData { get; } = new();

    protected virtual bool UseTestAuthentication => true;
    protected virtual string TestEnvironmentName => "Testing";

    public ApiTestWebApplicationFactory()
    {
        ApplyRequiredEnvironmentVariables();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment(TestEnvironmentName);

        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(GetConfigurationOverrides());
        });

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll(typeof(DbContextOptions<AppDbContext>));
            services.RemoveAll(typeof(IDbContextOptionsConfiguration<AppDbContext>));
            services.RemoveAll(typeof(AppDbContext));
            services.RemoveAll<IAppDbContext>();
            services.RemoveAll<IPaymentGateway>();
            services.RemoveAll<IMessagePublisher>();
            services.RemoveAll<INotificationService>();
            services.RemoveAll<IChatbotApiClient>();
            services.RemoveAll<IRecommendationApiClient>();

            var hostedServiceDescriptor = services.FirstOrDefault(d => d.ImplementationType == typeof(OutboxProcessor));
            if (hostedServiceDescriptor is not null)
                services.Remove(hostedServiceDescriptor);

            var fallbackRecoveryDescriptor = services.FirstOrDefault(d => d.ImplementationType == typeof(AiFallbackRecoveryProcessor));
            if (fallbackRecoveryDescriptor is not null)
                services.Remove(fallbackRecoveryDescriptor);

            services.AddDbContext<AppDbContext>((sp, options) =>
                options.AddInterceptors(sp.GetServices<ISaveChangesInterceptor>().Cast<IInterceptor>())
                    .UseInMemoryDatabase("BaytologyApiTests", _databaseRoot)
                    .UseInternalServiceProvider(_inMemoryEntityFrameworkProvider));
            services.AddScoped<IAppDbContext>(sp => sp.GetRequiredService<AppDbContext>());
            services.AddScoped<INotificationService, DatabaseNotificationService>();
            services.AddSingleton<IPaymentGateway, TestPaymentGateway>();
            services.AddSingleton<IMessagePublisher, TestMessagePublisher>();
            services.AddSingleton<IChatbotApiClient, TestChatbotApiClient>();
            services.AddSingleton<IRecommendationApiClient, TestRecommendationApiClient>();
            
            services.RemoveAll<IEmailSender>();
            services.AddSingleton<IEmailSender, TestEmailSender>();
            
            services.RemoveAll<IExternalLoginTokenValidator>();
            services.AddSingleton<IExternalLoginTokenValidator, TestExternalLoginTokenValidator>();

            if (UseTestAuthentication)
            {
                services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = TestAuthenticationHandler.SchemeName;
                    options.DefaultChallengeScheme = TestAuthenticationHandler.SchemeName;
                    options.DefaultScheme = TestAuthenticationHandler.SchemeName;
                }).AddScheme<AuthenticationSchemeOptions, TestAuthenticationHandler>(
                    TestAuthenticationHandler.SchemeName,
                    _ => { });
            }
        });
    }

    protected virtual IReadOnlyDictionary<string, string?> GetConfigurationOverrides()
    {
        return new Dictionary<string, string?>
        {
            ["ConnectionStrings:DefaultConnection"] = "Server=(localdb)\\mssqllocaldb;Database=Baytology_Test;Trusted_Connection=True;",
            ["JwtSettings:Secret"] = "Baytology_Test_Jwt_Secret_2026_ThatIsLongEnough_123456789",
            ["JwtSettings:Issuer"] = "Baytology.Api.Tests",
            ["JwtSettings:Audience"] = "Baytology.Api.Tests.Clients",
            ["AdminSettings:DefaultEmail"] = TestSeedData.AdminEmail,
            ["AdminSettings:DefaultPassword"] = TestSeedData.AdminPassword,
            ["RabbitMq:HostName"] = "localhost",
            ["RabbitMq:Port"] = "5672",
            ["RabbitMq:UserName"] = "guest",
            ["RabbitMq:Password"] = "guest",
            ["RabbitMq:Enabled"] = "false",
            ["Paymob:SecretKey"] = "secret-key",
            ["Paymob:PublicKey"] = "public-key",
            ["Paymob:IntegrationId"] = "123456",
            ["Paymob:WebhookToken"] = "webhook-token",
            ["Paymob:WebhookTokenQueryParameterName"] = "token",
            ["AiProcessing:EnableInProcessFallback"] = "true",
            ["AiProcessing:DefaultSearchResultLimit"] = "10",
            ["AiProcessing:DefaultRecommendationResultLimit"] = "10",
            ["AiWorker:ServiceToken"] = TestSeedData.AiWorkerServiceToken,
            ["AiWorker:ServiceTokenHeaderName"] = TestSeedData.AiWorkerServiceTokenHeaderName,
            ["GoogleAuthSettings:ClientId"] = "test-google-client-id",
            ["GoogleAuthSettings:ClientSecret"] = "test-google-client-secret",
            ["FacebookAuthSettings:AppId"] = "test-facebook-app-id",
            ["FacebookAuthSettings:AppSecret"] = "test-facebook-app-secret"
        };
    }

    public HttpClient CreateAuthenticatedClient(string userId, string email, params string[] roles)
    {
        var client = CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        client.DefaultRequestHeaders.Add(TestAuthenticationHandler.UserIdHeader, userId);
        client.DefaultRequestHeaders.Add(TestAuthenticationHandler.EmailHeader, email);
        client.DefaultRequestHeaders.Add(TestAuthenticationHandler.RolesHeader, string.Join(',', roles));
        return client;
    }

    public async Task ResetDatabaseAsync()
    {
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();

        await TestDataSeeder.SeedAsync(scope.ServiceProvider, SeedData);
    }

    public async Task InitializeAsync()
    {
        _ = Services;
        await ResetDatabaseAsync();
    }

    private static void ApplyRequiredEnvironmentVariables()
    {
        Environment.SetEnvironmentVariable("ConnectionStrings__DefaultConnection", "Server=(localdb)\\mssqllocaldb;Database=Baytology_Test;Trusted_Connection=True;");
        Environment.SetEnvironmentVariable("JwtSettings__Secret", "Baytology_Test_Jwt_Secret_2026_ThatIsLongEnough_123456789");
        Environment.SetEnvironmentVariable("JwtSettings__Issuer", "Baytology.Api.Tests");
        Environment.SetEnvironmentVariable("JwtSettings__Audience", "Baytology.Api.Tests.Clients");
        Environment.SetEnvironmentVariable("AdminSettings__DefaultEmail", TestSeedData.AdminEmail);
        Environment.SetEnvironmentVariable("AdminSettings__DefaultPassword", TestSeedData.AdminPassword);
        Environment.SetEnvironmentVariable("RabbitMq__HostName", "localhost");
        Environment.SetEnvironmentVariable("RabbitMq__Port", "5672");
        Environment.SetEnvironmentVariable("RabbitMq__UserName", "guest");
        Environment.SetEnvironmentVariable("RabbitMq__Password", "guest");
        Environment.SetEnvironmentVariable("RabbitMq__Enabled", "false");
        Environment.SetEnvironmentVariable("Paymob__SecretKey", "secret-key");
        Environment.SetEnvironmentVariable("Paymob__PublicKey", "public-key");
        Environment.SetEnvironmentVariable("Paymob__IntegrationId", "123456");
        Environment.SetEnvironmentVariable("Paymob__WebhookToken", "webhook-token");
        Environment.SetEnvironmentVariable("Paymob__WebhookTokenQueryParameterName", "token");
        Environment.SetEnvironmentVariable("AiProcessing__EnableInProcessFallback", "true");
        Environment.SetEnvironmentVariable("AiProcessing__DefaultSearchResultLimit", "10");
        Environment.SetEnvironmentVariable("AiProcessing__DefaultRecommendationResultLimit", "10");
        Environment.SetEnvironmentVariable("AiWorker__ServiceToken", TestSeedData.AiWorkerServiceToken);
        Environment.SetEnvironmentVariable("AiWorker__ServiceTokenHeaderName", TestSeedData.AiWorkerServiceTokenHeaderName);
    }

    Task IAsyncLifetime.DisposeAsync() => DisposeAsync().AsTask();
}
