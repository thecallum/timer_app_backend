using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using timer_app;
using timer_app.Infrastructure;

namespace timer_app_tests
{
    public class MockWebApplicationFactory : WebApplicationFactory<Startup>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                // Remove the existing DbContext configuration
                var descriptor = services.SingleOrDefault(d =>
                    d.ServiceType == typeof(DbContextOptions<TimerAppDbContext>));

                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                // Add DbContext with In-Memory database for testing
                services.AddDbContext<TimerAppDbContext>(options =>
                {
                    options.UseInMemoryDatabase("IntegrationTestDb");
                });
            });
        }

        public void CleanupDb()
        {
            using var dbContext = CreateDbContext();

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
