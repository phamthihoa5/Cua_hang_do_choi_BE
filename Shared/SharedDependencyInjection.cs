using Microsoft.Extensions.DependencyInjection;
using Shared.Services.ClaimService;

namespace Shared
{
    public static class SharedDependencyInjection
    {
        public static void AddSharedConfiguration(this IServiceCollection services)
        {
            services.AddScoped<IClaimService, ClaimService>();
        }
    }
}
