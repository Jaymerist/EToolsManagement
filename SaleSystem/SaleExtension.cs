#nullable disable
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SaleSystem.BLL;
using SaleSystem.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaleSystem
{
    public static class SaleExtension
    {
        public static void AddSalesDependencies(this IServiceCollection services, Action<DbContextOptionsBuilder> options)
        {
            services.AddDbContext<eTools2023Context>(options);

            services.AddTransient<SaleService>((ServiceProvider) =>
            {
                var context = ServiceProvider.GetService<eTools2023Context>();
                return new SaleService(context);
            });

            services.AddTransient<RefundService>((ServiceProvider) =>
            {
                var context = ServiceProvider.GetService<eTools2023Context>();
                return new RefundService(context);
            });
        }
    }
}
