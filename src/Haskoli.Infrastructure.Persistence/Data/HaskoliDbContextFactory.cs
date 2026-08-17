using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Haskoli.Infrastructure.Persistence.Data
{
    public  class HaskoliDbContextFactory : IDesignTimeDbContextFactory<HaskoliDbContext>
    {

        public HaskoliDbContext CreateDbContext(string[] args)
        {
            var configuration = new ConfigurationBuilder()
            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
            .AddJsonFile("appsettings.json")
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
