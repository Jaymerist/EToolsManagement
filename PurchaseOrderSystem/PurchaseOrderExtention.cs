using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PurchaseOrderSystem.DAL;
using PurchaseOrderSystem.BLL;

namespace PurchaseOrderSystem
{
    public static class PurchaseOrderExtention
    {
        public static void AddPurchaseOrderDependencies(this IServiceCollection services,
            Action<DbContextOptionsBuilder> options)
        {
            //  register the DBContext class in HogWildSystem with the service collection
            services.AddDbContext<eTools2023Context>(options);

            //  adding any services that you create in the class library (BLL)
            //  using .AddTransient<t>(...)
            //  customer
            services.AddTransient<VendorService>((ServiceProvider) =>
            {
                var context = ServiceProvider.GetService<eTools2023Context>();
                return new VendorService(context);
            });
            services.AddTransient<OrdersService>((ServiceProvider) =>
            {
                var context = ServiceProvider.GetService<eTools2023Context>();
                return new OrdersService(context);
            });
            services.AddTransient<InventoryService>((ServiceProvider) =>
            {
                var context = ServiceProvider.GetService<eTools2023Context>();
                return new InventoryService(context);
            });
            services.AddTransient<EmployeeService>((ServiceProvider) =>
            {
                var context = ServiceProvider.GetService<eTools2023Context>();
                return new EmployeeService(context);
            });
        }
    }
}
