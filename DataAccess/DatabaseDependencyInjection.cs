using DataAccess.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DataAccess;

/// <summary>
/// DatabaseDependencyInjection
/// </summary>
public static class DatabaseDependencyInjection
{
    public static void AddDatabaseConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<DatabaseConfiguration>(configuration.GetSection("Database"));

        var databaseConfig = configuration.GetSection("Database").Get<DatabaseConfiguration>();
        if (databaseConfig == null || string.IsNullOrEmpty(databaseConfig.Main))
            throw new Exception("Database configuration not found! Please check 'appsettings.json' file again.");
        services
            .AddDbContext<DatabaseContext>(options =>
                options.UseNpgsql(databaseConfig.Main,
                    opt => opt.MigrationsAssembly(typeof(DatabaseContext).Assembly.FullName)));

        //services.AddDbContext<DatabaseContext>(options =>
        //    options.UseMySql(
        //        databaseConfig.Main,
        //        ServerVersion.AutoDetect(databaseConfig.Main),
        //        opt => opt.MigrationsAssembly(typeof(DatabaseContext).Assembly.FullName)
        //    )
        //);
    }
}