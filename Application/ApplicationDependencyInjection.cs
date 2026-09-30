using Application.AppService;
using Application.AppService.User;
using Application.Interface;
using Application.IService.User;
using Application.MappingProfiles;
using Microsoft.Extensions.DependencyInjection;

namespace Application;

public static class ApplicationDependencyInjection
{
    public static void AddApplicationConfiguration(
        this IServiceCollection services)
    {
        // Unit Of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Quan ly nhan vien
        services.AddScoped<IStaffService, StaffService>();

        // Doc UserId tu JWT
        services.AddScoped<ITokenService, TokenService>();

        // HttpContext cho TokenService
        services.AddHttpContextAccessor();

        // AutoMapper
        services.AddAutoMapper(cfg =>
        {
            cfg.AddProfile<UserProfiles>();
        });
    }
}