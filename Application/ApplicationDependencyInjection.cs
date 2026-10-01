using Application.AppService;
using Application.AppService.Auth;
using Application.AppService.User;
using Application.Interface;
using Application.IService.Auth;
using Application.IService.User;
using Application.MappingProfiles;
using Application.MapperProfiles;
using Microsoft.Extensions.DependencyInjection;

namespace Application;

public static class ApplicationDependencyInjection
{
    public static void AddApplicationConfiguration(
        this IServiceCollection services)
    {
        // UNIT OF WORK
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // AUTH
        services.AddScoped<IAuthService, AuthService>();

        // STAFF
        services.AddScoped<IStaffService, StaffService>();
        services.AddScoped<ITokenService, TokenService>();

        // CLOUDINARY
        // Dùng để upload ảnh cho tin tức
        services.AddScoped<ICloudinaryService, CloudinaryService>();

        // NEWS
        services.AddScoped<INewsService, NewsService>();

        // PROMOTION
        services.AddScoped<IPromotionService, PromotionService>();

        // Tự động xử lý khuyến mãi hết hạn
        services.AddHostedService<PromotionCleanupService>();

        // HttpContext
        services.AddHttpContextAccessor();

        // AUTOMAPPER
        services.AddAutoMapper(cfg =>
        {
            cfg.AddProfile<UserProfiles>();
            cfg.AddProfile<NewsProfile>();
            cfg.AddProfile<PromotionProfile>();
        });
    }
}