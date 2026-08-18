using Haskoli.Domain.DTO;
using Haskoli.Domain.Entities;
using System.Linq.Expressions;

namespace Haskoli.Domain.Interfaces.Services
{
    public interface IStudentService
    {
        public int RowCount { get; }

        Task<IEnumerable<StudentDTO>> GetPagedStudents(int pageNumber, int pageSize, CancellationToken cancellationToken = default, Expression<Func<Student, bool>>? predicate = null, string? orderBy = null);

        Task<StudentDTO> FindStudent(int id, CancellationToken cancellationToken = default);

        Task<StudentDTO> CreateStudent(CreateStudentDTO student, CancellationToken cancellationToken = default);

        Task<StudentDTO> UpdateStudent(UpdateStudentDTO student, CancellationToken cancellationToken = default);

        Task<StudentDTO> DeleteStudent(DeleteStudentDTO student, CancellationToken cancellationToken = default);

        Task<bool> ExistsByDocument(string document, int? excludeId = null, CancellationToken cancellationToken = default);

        Task<bool> ExistsByEmail(string email, int? excludeId = null, CancellationToken cancellationToken = default);
    }
}
