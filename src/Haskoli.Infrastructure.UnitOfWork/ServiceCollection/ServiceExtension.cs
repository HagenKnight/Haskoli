using Microsoft.Extensions.DependencyInjection;
using Haskoli.Domain.Interfaces.Base;
using Haskoli.Infrastructure.Persistence.Data;
using Haskoli.Infrastructure.UnitOfWork.Base;


namespace Haskoli.Infrastructure.UnitOfWork.ServiceCollection
{
    public static class ServiceExtension
    {
        public static void AddUnitOfWorkLayer(this IServiceCollection services)
        {
            /* Factory & Unit Of Work. */
            services.AddScoped<IDbFactory<HaskoliDbContext>, DbFactory<HaskoliDbContext>>();
            services.AddScoped<IUnitOfWork<HaskoliDbContext>, UnitOfWork<HaskoliDbContext>>();
        }
    }
}
