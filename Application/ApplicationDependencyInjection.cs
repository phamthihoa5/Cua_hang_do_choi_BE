using Application.AppService;
using Application.AppService.Auth;
using Application.AppService.User;
using Application.Interface;
using Application.IService.Auth;
using Application.IService.User;
using Application.MappingProfiles;
using Microsoft.Extensions.DependencyInjection;

namespace Application;

public static class ApplicationDependencyInjection
{
    public static void AddApplicationConfiguration(
        this IServiceCollection services)
    {
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // AUTH
        services.AddScoped<IAuthService, AuthService>();

        // STAFF
        services.AddScoped<IStaffService, StaffService>();
        services.AddScoped<ITokenService, TokenService>();

        services.AddHttpContextAccessor();

        services.AddAutoMapper(cfg =>
        {
            cfg.AddProfile<UserProfiles>();
        });
    }
}