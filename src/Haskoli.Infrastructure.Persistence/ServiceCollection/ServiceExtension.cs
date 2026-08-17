using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Haskoli.Infrastructure.Persistence.Data;

namespace Haskoli.Infrastructure.Persistence.ServiceCollection
{
    public static class ServiceExtension
    {
        public static void AddPersistenceLayer(this IServiceCollection services, IConfiguration configuration)
        {
            /* Contextos de Bases de Datos. */
            string? connectionString = configuration.GetConnectionString("HaskoliConnection");
            string? dbSelector = configuration.GetValue<string>("Database");

            services.AddDbContext<HaskoliDbContext>(options =>
            {
                switch (dbSelector)
                {
                    case "mysql":
                        // MySQL support is temporarily parked: Pomelo.EntityFrameworkCore.MySql
                        // has no EF Core 10 release yet. Restore this branch when it ships.
                        throw new NotSupportedException("MySQL is temporarily unavailable until Pomelo.EntityFrameworkCore.MySql ships an EF Core 10 release. Use 'mssql'.");
                    case "mssql":
                    default:
                        options.UseSqlServer(connectionString);
                        break;
                }
            });

            /* DbFactory pattern. */
            /* Agregar aquí las implementaciones de Factory Pattern, asociadas a cada conexto de Base de Datos... */

            services.AddScoped<Func<HaskoliDbContext>>((provider) => () => provider.GetService<HaskoliDbContext>());
        }
    }
}
