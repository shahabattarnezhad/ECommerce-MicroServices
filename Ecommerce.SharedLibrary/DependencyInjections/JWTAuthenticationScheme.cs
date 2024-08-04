using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Ecommerce.SharedLibrary.DependencyInjections;

public static class JWTAuthenticationScheme
{
    public static IServiceCollection AddJWTAuthenticationScheme(
                                            this IServiceCollection services,
                                                 IConfiguration config)
    {
        // Add JWT Service
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer("Bearer", options =>
                {
                    var key =
                    Encoding.UTF8.GetBytes(config.GetSection("Authentication:Key").Value!);

                    var issuer =
                    config.GetSection("Authentication:Issuer").Value!;

                    var audience =
                    config.GetSection("Authentication:Audience").Value!;

                    options.RequireHttpsMetadata = false;
                    options.SaveToken = true;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = false,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = issuer,
                        ValidAudience = audience,
                        IssuerSigningKey = new SymmetricSecurityKey(key),
                    };
                });

        return services;
    }
}
