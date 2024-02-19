using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using timer_app.Boundary.Request;
using timer_app.Boundary.Request.Validation;
using timer_app.Gateway;
using timer_app.Gateway.Interfaces;
using timer_app.Gateways;
using timer_app.Gateways.Interfaces;
using timer_app.Infrastructure;
using timer_app.Middleware;
using timer_app.Middleware.Interfaces;
using timer_app.UseCases;
using timer_app.UseCases.Interfaces;

namespace timer_app;

public class Startup
{
    public Startup(IConfiguration configuration, IWebHostEnvironment env)
    {
        Configuration = configuration;
        _env = env;
    }

    public IConfiguration Configuration { get; }
    private readonly IWebHostEnvironment _env;

    // This method gets called by the runtime. Use this method to add services to the container
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddCors();
        services.AddControllers();

        ConfigureJwtAuthentication(services);

        ConfigureValidators(services);

        services.AddTransient<ICalendarEventsGateway, CalendarEventsGateway>();
        services.AddTransient<IProjectGateway, ProjectGateway>();
        services.AddTransient<IUserGateway, UserGateway>();

        services.AddScoped<IUserService, UserService>();

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

    private void ConfigureJwtAuthentication(IServiceCollection services)
    {
        var auth0Options = new Auth0Options();
        Configuration.Bind("Auth0", auth0Options);

        var isTestEnvironment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Test"
                          || Configuration["TestEnvironment"] == "True";

        if (!isTestEnvironment)
        {
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
            {
                options.Authority = auth0Options.Domain;
                options.Audience = auth0Options.Audience;

                //options.Authority = Environment.GetEnvironmentVariable("Auth0_Domain");
                //options.Audience = Environment.GetEnvironmentVariable("Auth0_Audience");

                options.IncludeErrorDetails = true;
                //options.

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    //  ValidateIssuer = true,
                    // ValidateAudience = true,
                    ValidateLifetime = true,
                    //  ValidateIssuerSigningKey = true,
                    ClockSkew = TimeSpan.Zero,

                };
            });
        }
    }

    private static void ConfigureValidators(IServiceCollection services)
    {
        services.AddScoped<IValidator<CreateEventRequest>, CreateEventRequestValidator>();
        services.AddScoped<IValidator<CreateProjectRequest>, CreateProjectRequestValidator>();
        services.AddScoped<IValidator<GetAllEventsRequest>, GetAllEventsRequestValidator>();
        services.AddScoped<IValidator<UpdateEventRequest>, UpdateEventRequestValidator>();
        services.AddScoped<IValidator<UpdateProjectRequest>, UpdateProjectRequestValidator>();
    }

    private void ConfigureDbContext(IServiceCollection services)
    {
        var connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING")
            ?? Configuration.GetValue<string>("CONNECTION_STRING");

        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

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

        app.UseCors(builder => builder
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod());

        if (Environment.GetEnvironmentVariable("LOCAL_ENV") == "true")
        {
            var scope = app.ApplicationServices.CreateScope();

            var context = scope.ServiceProvider.GetRequiredService<TimerAppDbContext>();
            context.Database.Migrate();
        }


        app.UseHttpsRedirection();

        app.UseRouting();

        app.UseAuthentication();

        app.UseAuthorization();
        app.UseMiddleware<UserMiddleware>();

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