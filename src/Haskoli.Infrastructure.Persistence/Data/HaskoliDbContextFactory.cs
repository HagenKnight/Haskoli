using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Haskoli.Infrastructure.Persistence.Data
{
    public  class HaskoliDbContextFactory : IDesignTimeDbContextFactory<HaskoliDbContext>
    {

        public HaskoliDbContext CreateDbContext(string[] args)
        {
            /* appsettings.json viaja con las cadenas en blanco por ser plantilla, así que sin el
               archivo del entorno las herramientas de EF se quedan sin cadena de conexión. */
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";

            var configuration = new ConfigurationBuilder()
            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
            .AddJsonFile("appsettings.json")
            .AddJsonFile($"appsettings.{environment}.json", optional: true)
            .Build();

            var optionsBuilder = new DbContextOptionsBuilder<HaskoliDbContext>();
            string connectionString = configuration.GetConnectionString("HaskoliConnection");
            string? dbSelector = configuration.GetValue<string>("Database");

            switch (dbSelector)
            {
                case "mysql":
                    // MySQL support is temporarily parked: Pomelo.EntityFrameworkCore.MySql
                    // has no EF Core 10 release yet. Restore this branch when it ships.
                    throw new NotSupportedException("MySQL is temporarily unavailable until Pomelo.EntityFrameworkCore.MySql ships an EF Core 10 release. Use 'mssql'.");
                case "mssql":
                default:
                    optionsBuilder.UseSqlServer(connectionString);
                    break;
            }

            return new HaskoliDbContext(optionsBuilder.Options);
        }
    }
}
