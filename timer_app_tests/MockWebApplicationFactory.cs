using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using timer_app;
using timer_app.Domain;
using timer_app.Gateways;
using timer_app.Gateways.Interfaces;
using timer_app.Infrastructure;

namespace timer_app_tests
{

    public class MockWebApplicationFactory : WebApplicationFactory<Startup>
    {
        protected Auth0User UserData = new Auth0User
        {
            Sub = "1234abcd",
            Nickname = "",
            Name = "",
            Picture = "",
            UpdatedAt = "",
            Email = "",
            EmailVerified = true
        };

    protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.IntegrationTests.json")
                .AddEnvironmentVariables()
                .Build();

            builder.ConfigureAppConfiguration((context, configBuilder) =>
            {
                configBuilder.AddInMemoryCollection(new Dictionary<string, string>
                {
                    ["TestEnvironment"] = "True"
                });
            });

            builder.ConfigureServices(services =>
            {
                services.RemoveAll(typeof(DbContextOptions<TimerAppDbContext>));
                services.RemoveAll<TimerAppDbContext>();

                var connectionString = configuration["CONNECTION_STRING"];
                services.AddDbContext<TimerAppDbContext>(options => options.UseNpgsql(connectionString));

                var serviceProvider = services.BuildServiceProvider();
                var scope = serviceProvider.CreateScope();

                var dbContext = scope.ServiceProvider.GetRequiredService<TimerAppDbContext>();
                dbContext.Database.EnsureDeleted();
                dbContext.Database.EnsureCreated();
                dbContext.SaveChanges();


                services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
                {
                    options.IncludeErrorDetails = true;

                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = "https://your-auth0-domain/", // Match this with your token's issuer

                        ValidateAudience = true,
                        ValidAudience = "https://localhost:50086/", // Match this with your token's audience

                        ValidateLifetime = true, // to validate the expiration and not before values in the token

                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("89f6ac7859e58d5ef58a780b04a7757b0e2a0f5cf66168de1f95b8b2e0ab03cb")), // Use the same key you used to sign your test tokens

                        // Ensure the clock skew is reasonable to prevent issues with token expiration
                        ClockSkew = TimeSpan.Zero
                    };
                });



                services.RemoveAll<UserGateway>();
                services.AddTransient<IUserGateway>(x =>
                {
                    var instance = new MockUserGateway(UserData);

                    return instance;
                });

            })
            .UseEnvironment("IntegrationTests");
        }

        protected void CleanupDb()
        {
            using var dbContext = CreateDbContext();

            dbContext.CalendarEvents.RemoveRange(dbContext.CalendarEvents);
            dbContext.SaveChanges();

            dbContext.Projects.RemoveRange(dbContext.Projects);
            dbContext.SaveChanges();
        }

        protected TimerAppDbContext CreateDbContext()
        {
            var scope = Services.CreateScope();
            return scope.ServiceProvider.GetRequiredService<TimerAppDbContext>();
        }

        protected static string GenerateToken()
        {
            var token = TokenHelper.GenerateToken(
                  issuer: "https://your-auth0-domain/",
                  audience: "https://localhost:50086/",
                  expiry: DateTime.UtcNow.AddHours(1),
                  claims: new Dictionary<string, string>
                  {
                                { ClaimTypes.NameIdentifier, "test-user-id" },
                                { ClaimTypes.Email, "test@example.com" }
                      // Add other claims as needed
                  },
                  signingKey: "89f6ac7859e58d5ef58a780b04a7757b0e2a0f5cf66168de1f95b8b2e0ab03cb"
              );

            return token;
        }
    }
}
