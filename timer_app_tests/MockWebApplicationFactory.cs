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

        private const string TokenIssuer = "https://your-auth0-domain/";
        private const string TokenAudience = "https://localhost:50086/";
        private const string TokenKey = "89f6ac7859e58d5ef58a780b04a7757b0e2a0f5cf66168de1f95b8b2e0ab03cb";

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
                ConfigureDbContext(services, configuration);
                ConfigureAuthentication(services);
                ConfigureMockUserGateway(services);
            })
            .UseEnvironment("IntegrationTests");
        }

        private static void ConfigureDbContext(IServiceCollection services, IConfigurationRoot configuration)
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
        }

        private static void ConfigureAuthentication(IServiceCollection services)
        {
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
            {
                options.IncludeErrorDetails = true;

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = TokenIssuer,
                    ValidateAudience = true,
                    ValidAudience = TokenAudience,
                    ValidateLifetime = true, 
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(TokenKey)),
                    ClockSkew = TimeSpan.Zero
                };
            });
        }

        private void ConfigureMockUserGateway(IServiceCollection services)
        {
            services.RemoveAll<UserGateway>();
            services.AddTransient<IUserGateway>(x => new MockUserGateway(UserData));
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
            var expiry = DateTime.UtcNow.AddHours(1);
            var claims = new Dictionary<string, string>();

            return TokenHelper.GenerateToken(TokenIssuer, TokenAudience, expiry, claims, TokenKey);
        }
    }
}
