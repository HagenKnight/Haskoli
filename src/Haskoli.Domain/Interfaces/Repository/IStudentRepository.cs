using Haskoli.Domain.Entities;
using Haskoli.Domain.Interfaces.Base;
using Microsoft.EntityFrameworkCore;

namespace Haskoli.Domain.Interfaces.Repository
{
    public interface IStudentRepository<TContext> : IBaseRepository<Student, TContext> where TContext : DbContext, new()
    {
        /// <summary>
        /// Alcanza también a los estudiantes eliminados lógicamente, porque el índice único
        /// de la base de datos los abarca y su documento no se libera.
        /// </summary>
        Task<bool> ExistsByDocumentAsync(string document, int? excludeId = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Alcanza también a los estudiantes eliminados lógicamente, porque el índice único
        /// de la base de datos los abarca y su email no se libera.
        /// </summary>
        Task<bool> ExistsByEmailAsync(string email, int? excludeId = null, CancellationToken cancellationToken = default);
    }
}
