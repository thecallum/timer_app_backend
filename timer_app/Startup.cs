using FluentValidation;
using Microsoft.EntityFrameworkCore;
using System;
using System.Reflection;
using timer_app.Boundary.Request;
using timer_app.Boundary.Request.Validation;
using timer_app.Gateway;
using timer_app.Gateway.Interfaces;
using timer_app.Infrastructure;
using timer_app.UseCases;
using timer_app.UseCases.Interfaces;

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

        services.AddTransient<ICalendarEventsGateway, CalendarEventsGateway>();
        services.AddTransient<IProjectGateway, ProjectGateway>();

        services.AddTransient<ICreateEventUseCase, CreateEventUseCase>();
        services.AddTransient<ICreateProjectUseCase, CreateProjectUseCase>();
        services.AddTransient<IDeleteEventUseCase, DeleteEventUseCase>();
        services.AddTransient<IDeleteProjectUseCase, DeleteProjectUseCase>();
        services.AddTransient<IGetAllEventsUseCase, GetAllEventsUseCase>();
        services.AddTransient<IGetAllProjectsUseCase, GetAllProjectsUseCase>();
        services.AddTransient<IUpdateEventUseCase, UpdateEventUseCase>();
        services.AddTransient<IUpdateProjectUseCase, UpdateProjectUseCase>();


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

        services.AddDbContext<TimerAppDbContext>(
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