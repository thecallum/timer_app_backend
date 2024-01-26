using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using timer_app;
using timer_app.Infrastructure;

namespace timer_app_tests
{
    public class MockWebApplicationFactory : WebApplicationFactory<Startup>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.IntegrationTests.json")
                .AddEnvironmentVariables()
                .Build();

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
            })
            .UseEnvironment("IntegrationTests");
        }

        public void CleanupDb()
        {
            using var dbContext = CreateDbContext();

            dbContext.CalendarEvents.RemoveRange(dbContext.CalendarEvents);
            dbContext.SaveChanges();

            dbContext.Projects.RemoveRange(dbContext.Projects);
            dbContext.SaveChanges();
        }

        public TimerAppDbContext CreateDbContext()
        {
            var scope = Services.CreateScope();
            return scope.ServiceProvider.GetRequiredService<TimerAppDbContext>();
        }
    }
}
