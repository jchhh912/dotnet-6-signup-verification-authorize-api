using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;

public static class JwtIdentificationExtensions
{
    internal static IServiceCollection AddJwtIdentification(this IServiceCollection services, IConfiguration config)
    {
        services.Configure<JwtSettings>(config.GetSection(nameof(JwtSettings)))
            .AddIdentity<AppUser, IdentityRole<Guid>>(options =>
        {
            //https://blog.csdn.net/sD7O95O/article/details/106464068
            //options https://docs.microsoft.com/zh-cn/aspnet/core/security/authentication/identity-configuration?view=aspnetcore-2.0&amp;tabs=aspnetcore2x
           
            //密码长度 配置
            options.Password.RequiredLength = 6;
            options.Password.RequireDigit = false;
            options.Password.RequireLowercase = false;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequireUppercase = false;
            //唯一邮箱
            options.User.RequireUniqueEmail = true;
            //账号可取字符范围
            options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789.@";
        })
        .AddEntityFrameworkStores<BaseDbContext>()
        .AddDefaultTokenProviders();
       
        var jwtSettings = services.GetOptions<JwtSettings>(nameof(JwtSettings));
        byte[] key = Encoding.ASCII.GetBytes(jwtSettings.Key);
        _= services
            .AddAuthentication(authentication => 
        {
            authentication.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            authentication.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(bearer => 
        {
            //将JWT保存到当前的HttpContext, 以至于可以获取它通过await HttpContext.GetTokenAsync("Bearer","access_token"); 如果想设置为false, 将token保存在claim中, 然后获取通过User.FindFirst("access_token")?.value.
            //bearer.RequireHttpsMetadata = false;
            //bearer.SaveToken = true;
            bearer.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidateLifetime = true,
                ValidateAudience = true,
                ValidIssuer = jwtSettings.Issuer,
                ValidAudience = jwtSettings.Audience,
                RoleClaimType = ClaimTypes.Role,
                ClockSkew = TimeSpan.Zero
            };
            bearer.Events = new JwtBearerEvents
            {
                OnChallenge = context =>
                {
                    context.HandleResponse();
                    if (!context.Response.HasStarted)
                    {
                        throw new UnauthorizedAccessException("Authentication Failed(认证失败9001).");
                    }

                    return Task.CompletedTask;
                },
                OnForbidden = _ =>
                {
                    throw new UnauthorizedAccessException("You are not authorized to access this resource(您未被授权访问此资源).");
                },
                OnAuthenticationFailed = context =>
                {

                    throw new UnauthorizedAccessException("Authentication Failed(认证失败9002).");
                }
            };
        });
        return services;
    }
}
