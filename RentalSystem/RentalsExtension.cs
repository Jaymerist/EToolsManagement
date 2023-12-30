using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RentalSystem.BLL;
using RentalSystem.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RentalSystem
{
    public static class RentalsExtension
    {
        public static void AddRentalsDependencies(this IServiceCollection services,
            Action<DbContextOptionsBuilder> options)
        {
            //  register the DBContext class in HogWildSystem with the service collection
            services.AddDbContext<eTools2023Context>(options);

            services.AddTransient<RentService>((ServiceProvider) =>
            {
                var context = ServiceProvider.GetService<eTools2023Context>();
                return new RentService(context);
            });

            services.AddTransient<ReturnService>((ServiceProvider) =>
            {
                var context = ServiceProvider.GetService<eTools2023Context>();
                return new ReturnService(context);
            });

            services.AddTransient<CustomerService>((ServiceProvider) =>
            {
                var context = ServiceProvider.GetService<eTools2023Context>();
                return new CustomerService(context);
            });

        }
    }
}
