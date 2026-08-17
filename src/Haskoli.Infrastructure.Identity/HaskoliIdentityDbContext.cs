using Haskoli.Infrastructure.Identity.Configurations;
using Haskoli.Infrastructure.Identity.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Haskoli.Infrastructure.Identity
{
    public class HaskoliIdentityDbContext : IdentityDbContext<ApplicationUser>
    {
        private string _dbSelector = string.Empty;
        public string Collation { get; set; }

        const string SqlServerCollation = "Modern_Spanish_CI_AS";

        public HaskoliIdentityDbContext(DbContextOptions options) : base(options)
        {
            var extension = options.FindExtension<Microsoft.EntityFrameworkCore.SqlServer.Infrastructure.Internal.SqlServerOptionsExtension>();

            if (extension != null)
            {
                _dbSelector = "mssql";
            }
            else
            {
                throw new InvalidOperationException("No valid database provider found in options.");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configura la collation por defecto para todas las tablas
            switch (_dbSelector)
            {
                case "mssql":
                default:
                    Collation = SqlServerCollation;
                    break;
            }

            base.OnModelCreating(modelBuilder);

            // collation by default to every table
            modelBuilder.UseCollation(Collation);

            // Configura la collation para todas las columnas de texto
            foreach (var entity in modelBuilder.Model.GetEntityTypes())
            {
                foreach (var property in entity.GetProperties())
                {
                    if (property.ClrType == typeof(string))
                    {
                        property.SetCollation(Collation);
                    }
                }
            }

            modelBuilder.ApplyConfiguration(new UserConfiguration());
            modelBuilder.ApplyConfiguration(new RoleConfiguration());
            modelBuilder.ApplyConfiguration(new UserRoleConfiguration());
        }

    }
}
