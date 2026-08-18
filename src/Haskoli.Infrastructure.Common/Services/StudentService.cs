using AutoMapper;
using Haskoli.Domain.DTO;
using Haskoli.Domain.DTO.Base;
using Haskoli.Domain.Entities;
using Haskoli.Domain.Interfaces.Base;
using Haskoli.Domain.Interfaces.Repository;
using Haskoli.Domain.Interfaces.Services;
using Haskoli.Infrastructure.Persistence.Data;
using Haskoli.Infrastructure.Persistence.Services.Base;
using System.Linq.Expressions;

namespace Haskoli.Infrastructure.Common.Services
{
    public class StudentService : CRUDService<Student, StudentDTO, CommandDTO, int,
                                  Student, IStudentRepository<HaskoliDbContext>, HaskoliDbContext>,
                                  IStudentService
    {
        public StudentService(IStudentRepository<HaskoliDbContext> repository,
            IUnitOfWork<HaskoliDbContext> unitOfWork,
            IMapper mapper
            ) : base(repository,
                unitOfWork,
                mapper)
        {
        }

        /* Se pasa cadena vacía y no null porque los parámetros del servicio genérico no están
           anotados como nulables; ambos se comprueban con IsNullOrEmpty, así que el efecto es
           el mismo. Sin filtro se usa la sobrecarga sin predicado para no contar de más. */
        public async Task<IEnumerable<StudentDTO>> GetPagedStudents(int pageNumber, int pageSize, CancellationToken cancellationToken = default, Expression<Func<Student, bool>>? predicate = null, string? orderBy = null) =>
            (predicate == null) ? await GetPagedAsync(pageNumber, pageSize, cancellationToken, string.Empty, orderBy ?? string.Empty)
                                : await GetPagedAsync(pageNumber, pageSize, predicate, cancellationToken, string.Empty, orderBy ?? string.Empty);

        public async Task<StudentDTO> FindStudent(int id, CancellationToken cancellationToken = default) =>
            await FindAsync(id, cancellationToken);

        public async Task<StudentDTO> CreateStudent(CreateStudentDTO student, CancellationToken cancellationToken = default) =>
            await InsertAsync(student, cancellationToken);

        public async Task<StudentDTO> UpdateStudent(UpdateStudentDTO student, CancellationToken cancellationToken = default) =>
            await UpdateAsync(student, cancellationToken);

        public async Task<StudentDTO> DeleteStudent(DeleteStudentDTO student, CancellationToken cancellationToken = default) =>
            await DeleteAsync(student, true, cancellationToken);

        public async Task<bool> ExistsByDocument(string document, int? excludeId = null, CancellationToken cancellationToken = default) =>
            await Repository.ExistsByDocumentAsync(document, excludeId, cancellationToken);

        public async Task<bool> ExistsByEmail(string email, int? excludeId = null, CancellationToken cancellationToken = default) =>
            await Repository.ExistsByEmailAsync(email, excludeId, cancellationToken);
    }
}
