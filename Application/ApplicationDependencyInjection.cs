using Application.AppService;
using Application.AppService.Auth;
using Application.AppService.User;
using Application.Interface;
using Application.IService.Auth;
using Application.IService.User;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Application;

/// <summary>
/// Application Dependency Injection
/// </summary>
public static class ApplicationDependencyInjection
{
    /// <summary>
    /// Add Services Dependency Injection
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    public static void AddApplicationConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        // Init server settings
        //services.Configure<ServerSettings>(configuration.GetSection("Server"));
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<ICloudinaryService, CloudinaryService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<INewsService, NewsService>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<ICartService, CartService>();
        services.AddScoped<IPromotionService, PromotionService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<ISupplierService, SupplierService>();
        services.AddScoped<IWarehouseService, WarehouseService>();
        services.AddScoped<IStatisticService, StatisticService>();
        services.AddScoped<IVnPayService, VnPayService>();
        services.AddScoped<IPaymentService, PaymentService>();
        services.AddScoped<IPermissionService, PermissionService>();
        services.AddScoped<IStaffService, StaffService>();
        services.AddScoped<ICustomerService, CustomerService>();
        services.AddScoped<IGhnService, GhnService>();
        services.AddScoped<IAiService, AiService>();
        services.AddHostedService<PromotionCleanupService>();
    }
}
