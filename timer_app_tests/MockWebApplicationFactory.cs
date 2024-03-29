﻿using AutoFixture;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using timer_app;
using timer_app.Domain;
using timer_app.Infrastructure;
using timer_app.Middleware;

namespace timer_app_tests
{

    public class MockTokenValidator : ITokenValidator
    {
        private readonly Auth0User _userData;

        public MockTokenValidator(Auth0User userData)
        {
            _userData = userData;
        }

        public async Task<ClaimsPrincipal> ValidateIdToken(string idToken)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, _userData.Id)
            };

            var claimsIdentity = new ClaimsIdentity(claims);

            // Create a ClaimsPrincipal using the ClaimsIdentity
            var principal = new ClaimsPrincipal(claimsIdentity);

            return principal;
        }
    }



    public class MockWebApplicationFactory : WebApplicationFactory<Startup>
    {
        protected readonly Fixture _fixture = new Fixture();
        protected readonly Random _random = new Random();

        private const string TokenIssuer = "TOKEN_ISSUER";
        private const string TokenAudience = "TOKEN_AUDIENCE";
        private const string TokenKey = "89f6ac7859e58d5ef58a780b04a7757b0e2a0f5cf66168de1f95b8b2e0ab03cb";

        protected Auth0User UserData;

        public MockWebApplicationFactory()
        {
            UserData = _fixture.Create<Auth0User>();
        }

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
                ConfigureMockTokenValidator(services);
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

        private void ConfigureMockTokenValidator(IServiceCollection services)
        {
            services.RemoveAll<ITokenValidator>();
            services.AddTransient<ITokenValidator>(x => new MockTokenValidator(UserData));
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

        protected static string GenerateAccessToken()
        {
            var expiry = DateTime.UtcNow.AddHours(1);
            // var claims = new Dictionary<string, string>();

            return TokenHelper.GenerateAccessToken(TokenIssuer, TokenAudience, expiry, TokenKey);
        }

        protected static string GenerateIdToken()
        {
            return "ID_TOKEN_VALUE";
        }
    }
}
