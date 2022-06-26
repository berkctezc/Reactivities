using System.Text;
using API.Services;
using Domain;
using Infrastructure.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Persistence;
using AuthenticationServiceCollectionExtensions = Microsoft.Extensions.DependencyInjection.AuthenticationServiceCollectionExtensions;
using IdentityEntityFrameworkBuilderExtensions = Microsoft.Extensions.DependencyInjection.IdentityEntityFrameworkBuilderExtensions;
using JwtBearerExtensions = Microsoft.Extensions.DependencyInjection.JwtBearerExtensions;

namespace API.Extensions;

public static class IdentityServiceExtensions
{
    public static IServiceCollection AddIdentityServices(this IServiceCollection services,
        IConfiguration config)
    {
        IdentityEntityFrameworkBuilderExtensions.AddEntityFrameworkStores<DataContext>(services.AddIdentityCore<AppUser>(opt =>
        {
            opt.Password.RequireNonAlphanumeric = false;
        }))
        .AddSignInManager<SignInManager<AppUser>>();

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["TokenKey"]));

        JwtBearerExtensions.AddJwtBearer(AuthenticationServiceCollectionExtensions.AddAuthentication(services, JwtBearerDefaults.AuthenticationScheme), opt =>
            {
                opt.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = key,
                    ValidateIssuer = false,
                    ValidateAudience = false
                };
            });

        services.AddAuthorization(opt =>
        {
            opt.AddPolicy("IsActivityHost", policy =>
            {
                policy.Requirements.Add(new IsHostRequirement());
            });
        });
        services.AddTransient<IAuthorizationHandler, IsHostRequirementHandler>();
        services.AddScoped<TokenService>();

        return services;
    }
}