using Application.AppService.User;
using Application.IService.User;
using Microsoft.Extensions.DependencyInjection;

namespace Application;

public static class ApplicationDependencyInjection
{
    public static void AddApplicationConfiguration(
        this IServiceCollection services)
    {
        services.AddScoped<IStaffService, StaffService>();
    }
}