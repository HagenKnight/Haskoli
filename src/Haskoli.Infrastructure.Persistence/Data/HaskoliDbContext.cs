using Haskoli.Domain.Entities;
using Haskoli.Domain.Entities.Base;
using Haskoli.Infrastructure.Persistence.Data.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Haskoli.Infrastructure.Persistence.Data
{
    public class HaskoliDbContext : DbContext
    {
        private string _dbSelector = string.Empty;
        private string Collation;

        const string SqlServerCollation = "Modern_Spanish_CI_AS";

        public HaskoliDbContext() : base() { }
        public HaskoliDbContext(DbContextOptions<HaskoliDbContext> options) : base(options)
        {
            var extension = options.FindExtension<Microsoft.EntityFrameworkCore.SqlServer.Infrastructure.Internal.SqlServerOptionsExtension>();

            if (extension != null)
                _dbSelector = "mssql";
            else
                throw new InvalidOperationException("No valid database provider found in options.");
        }

        // Dbset for Entities */
        public DbSet<Country> Countries { get; set; }

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

            modelBuilder.UseCollation(Collation);
            // Singularize table name
            foreach (IMutableEntityType entity in modelBuilder.Model.GetEntityTypes())
            {
                if (entity.BaseType == null)
                    entity.SetTableName(entity.DisplayName());
            }

            modelBuilder.ApplyConfiguration(new CountryConfiguration());
        }


        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            foreach (var entry in ChangeTracker.Entries<EntityBase<int>>())
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.CreatedDate = DateTime.UtcNow;
                        entry.Entity.CreatedBy = "system";
                        entry.Entity.IsDeleted = false;
                        break;

                    case EntityState.Modified:
                        entry.Entity.LastModifiedDate = DateTime.UtcNow;
                        entry.Entity.LastModifiedBy = "system";
                        break;
                }
            }

            return base.SaveChangesAsync(cancellationToken);
        }

    }
}