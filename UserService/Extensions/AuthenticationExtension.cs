using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using UserService.Model.Settings;

namespace UserService.Extensions;

public static class AuthenticationExtension
{
    public static WebApplicationBuilder AddAuthenticationAndAuthorization(
        this WebApplicationBuilder builder
    )
    {
        var jwtOptions = builder.Configuration.GetSection("JwtOptions").Get<JwtSettings>();

        var key = Encoding.ASCII.GetBytes(jwtOptions!.Secret);

        builder
            .Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(x =>
            {
                x.SaveToken = true;
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = jwtOptions.Issuer,
                    ValidAudience = jwtOptions.Audience,
                    ValidateAudience = true,
                };
            });
        builder.Services.AddAuthorization();

        return builder;
    }
}
