using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ReceivingSystem.DAL;
using ReceivingSystem.BLL; // Add this using directive to access your BLL classes

namespace ReceivingSystem
{
    public static class ReceivingSystemDependencies
    {
        public static IServiceCollection AddReceivingDependencies(this IServiceCollection services, Action<DbContextOptionsBuilder> options)
        {
            // Register the DbContext
            services.AddDbContext<eTools2023Context>(options);

            services.AddScoped<PurchaseOrderService>();


            return services;
        }
    }
}
