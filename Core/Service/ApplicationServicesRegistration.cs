using Microsoft.Extensions.DependencyInjection;
using ServiceAbstraction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service
{
    public static class ApplicationServicesRegistration
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection Services)
        {
            Services.AddAutoMapper(typeof(AssemblyReference).Assembly);
            Services.AddScoped<IServiceManager, ServiceManagerWithFactoryDelegate>();
            Services.AddScoped<IProductService, ProductService>();
            Services.AddScoped<Func<IProductService>>(Provider => () => Provider.GetRequiredService<IProductService>());

            Services.AddScoped<IOrderService,OrderService>();
            Services.AddScoped<Func<IOrderService>>(Provider=>()=>Provider.GetRequiredService<IOrderService>());

            Services.AddScoped<IAuthenticationService, AuthenticationService>();
            Services.AddScoped<Func<IAuthenticationService>>(Provider => () => Provider.GetRequiredService<IAuthenticationService>());

            Services.AddScoped<IBasketService, BasketService>();
            Services.AddScoped<Func<IBasketService>>(Provider => () => Provider.GetRequiredService<IBasketService>());

            Services.AddScoped<ICacheService, CacheService>();

            Services.AddScoped<IPaymentService, PaymentService>();
            Services.AddScoped<Func<IPaymentService>>(Provider => () => Provider.GetRequiredService<IPaymentService>());

            return Services;
        }
    }
}
