using Haskoli.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Haskoli.Infrastructure.Persistence.Data.Configurations
{
    public class StudentConfiguration : IEntityTypeConfiguration<Student>
    {
        public void Configure(EntityTypeBuilder<Student> builder)
        {
            builder.ToTable("Student");

            /* El índice abarca también a los eliminados lógicamente: su documento y su email
               no se liberan, así que la verificación de unicidad debe consultarlos con
               IgnoreQueryFilters para no chocar contra estos índices. */
            builder.HasIndex(s => s.Document).IsUnique();
            builder.HasIndex(s => s.Email).IsUnique();

            builder.HasQueryFilter(s => !s.IsDeleted);
        }
    }
}
