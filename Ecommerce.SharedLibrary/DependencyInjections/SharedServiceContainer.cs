using Ecommerce.SharedLibrary.Middlewares;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Ecommerce.SharedLibrary.DependencyInjections;

public static class SharedServiceContainer
{
    public static IServiceCollection AddSharedServices<TContext>
        (this IServiceCollection services, IConfiguration config, string fileName)
        where TContext : DbContext
    {
        // Add generic databse context:
        services.AddDbContext<TContext>(options =>
        {
            options.UseSqlServer(config.GetConnectionString("connection"),
                sqlServerOptions =>
                {
                    sqlServerOptions.EnableRetryOnFailure();
                });
        });

        // Configure serilog logging:
        Log.Logger = new LoggerConfiguration()
                              .MinimumLevel
                              .Information()
                              .WriteTo
                              .Debug()
                              .WriteTo
                              .Console()
                              .WriteTo
                              .File(path: $"{fileName}-.txt",
                                    restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Information,
                                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {message:lj}{NewLine}{Exception}",
                                    rollingInterval: RollingInterval.Day)
                              .CreateLogger();

        // Add JWT authentication scheme:
        JWTAuthenticationScheme.AddJWTAuthenticationScheme(services, config);

        return services;
    }

    public static IApplicationBuilder UseSharedPolicies(this IApplicationBuilder app)
    {
        // Use global exception:
        app.UseMiddleware<GlobalException>();

        // Register middleware to block all outsiders API calls:
        app.UseMiddleware<ListenToOnlyApiGateway>();

        return app;
    }
}
