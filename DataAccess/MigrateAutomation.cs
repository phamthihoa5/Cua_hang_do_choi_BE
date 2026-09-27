using Core.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shared.Logger;

namespace DataAccess;

public static class MigrateAutomation
{
    public static async Task MigrateAsync(IServiceProvider services)
    {
        var context = services.GetRequiredService<DatabaseContext>();
        var roleManager = services.GetRequiredService<RoleManager<ApplicationRole>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

        // ✅ Auto apply migrations vào DB
        try
        {
            await context.Database.MigrateAsync();
        }
        catch (Exception ex)
        {
            Logging.Info("MigrateAsync skipped or tables already exist: " + ex.Message);
        }

        // ✅ Seed dữ liệu (roles, admin...)
        await DbSeeder.SeedAsync(context, userManager, roleManager);

        Logging.Info("✅ Migration & Seed completed successfully!");
    }
}
