using Microsoft.EntityFrameworkCore;
using timer_app.Infrastructure;

namespace timer_app;

public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    // This method gets called by the runtime. Use this method to add services to the container
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers();

        ConfigureDbContext(services);
    }

    static void ConfigureDbContext(IServiceCollection services)
    {
        var connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING");

        if (string.IsNullOrEmpty(connectionString))
        {
            //throw new InvalidOperationException($"The required environment variable 'CONNECTION_STRING' is not set.");
        }

        services.AddDbContext<TimerAppContext>(
            opt => opt
                .UseNpgsql(connectionString)
        );
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseHttpsRedirection();

        app.UseRouting();

        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
            endpoints.MapGet("/", async context =>
            {
                await context.Response.WriteAsync("Welcome to running ASP.NET Core on AWS Lambda");
            });
        });
    }
}