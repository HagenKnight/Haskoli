using Haskoli.Domain.Entities;
using Haskoli.Domain.Interfaces.Base;
using Haskoli.Domain.Interfaces.Repository;
using Haskoli.Infrastructure.Persistence.Data;
using Haskoli.Infrastructure.Persistence.Repository.Base;
using Microsoft.EntityFrameworkCore;

namespace Haskoli.Infrastructure.Common.Repositories
{
    public class StudentRepository : BaseRepository<Student, int, HaskoliDbContext>, IStudentRepository<HaskoliDbContext>
    {
        public StudentRepository(IDbFactory<HaskoliDbContext> dbFactory) : base(dbFactory) { }

        /* Ambas verificaciones ignoran el filtro global de eliminados: los índices únicos de la
           base abarcan también a los estudiantes eliminados, de modo que consultarlos sin
           IgnoreQueryFilters daría vía libre a una inserción que la base terminaría rechazando. */

        public async Task<bool> ExistsByDocumentAsync(string document, int? excludeId = null, CancellationToken cancellationToken = default) =>
            await DbContext.Set<Student>()
                .IgnoreQueryFilters()
                .AnyAsync(s => s.Document == document && (excludeId == null || s.Id != excludeId.Value), cancellationToken);

        public async Task<bool> ExistsByEmailAsync(string email, int? excludeId = null, CancellationToken cancellationToken = default) =>
            await DbContext.Set<Student>()
                .IgnoreQueryFilters()
                .AnyAsync(s => s.Email == email && (excludeId == null || s.Id != excludeId.Value), cancellationToken);
    }
}
