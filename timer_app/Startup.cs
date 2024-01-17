using FluentValidation;
using Microsoft.EntityFrameworkCore;
using System;
using timer_app.Boundary.Request;
using timer_app.Boundary.Request.Validation;
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

        ConfigureValidators(services);

        ConfigureDbContext(services);
    }

    private static void ConfigureValidators(IServiceCollection services)
    {
        services.AddScoped<IValidator<CreateEventRequest>, CreateEventRequestValidator>();
        services.AddScoped<IValidator<CreateProjectRequest>, CreateProjectRequestValidator>();
        services.AddScoped<IValidator<GetAllEventsRequest>, GetAllEventsRequestValidator>();
        services.AddScoped<IValidator<UpdateEventRequest>, UpdateEventRequestValidator>();
        services.AddScoped<IValidator<UpdateProjectRequest>, UpdateProjectRequestValidator>();
    }

    static void ConfigureDbContext(IServiceCollection services)
    {
        var connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING");

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