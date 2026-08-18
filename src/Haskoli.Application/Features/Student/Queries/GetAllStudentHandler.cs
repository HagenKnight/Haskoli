using AutoMapper;
using Haskoli.Domain.Custom;
using Haskoli.Domain.DTO;
using Haskoli.Domain.Interfaces.Management;
using Haskoli.Domain.Interfaces.Services;
using Haskoli.Domain.Parameters;
using Haskoli.Domain.Wrappers;
using MediatR;
using System.Linq.Expressions;
using StudentEntity = Haskoli.Domain.Entities.Student;

namespace Haskoli.Application.Features.Student
{
    public class GetAllStudentHandler : IRequestHandler<GetAllStudentQuery, ApiResponse<MetaData<StudentDTO>>>
    {
        private readonly IMapper _mapper;
        private readonly IUriService _uriService;
        private readonly IStudentService _studentService;

        public GetAllStudentHandler(IStudentService studentService, IMapper mapper, IUriService uriService) =>
            (_studentService, _mapper, _uriService) = (studentService, mapper, uriService);

        public async Task<ApiResponse<MetaData<StudentDTO>>> Handle(GetAllStudentQuery request, CancellationToken cancellationToken)
        {
            GetAllStudentParameter filter = _mapper.Map<GetAllStudentParameter>(request);

            IEnumerable<StudentDTO> students = await _studentService.GetPagedStudents(
                filter.PageNumber, filter.PageSize, cancellationToken, BuildPredicate(filter), filter.OrderBy);

            PagedList<StudentDTO> pagedStudents = new(students, filter.PageNumber, filter.PageSize,
                _studentService.RowCount, _uriService, string.Empty, filter.OrderBy ?? string.Empty, string.Empty, request.Route ?? string.Empty);

            return new ApiResponse<MetaData<StudentDTO>>(_mapper.Map<PagedList<StudentDTO>, MetaData<StudentDTO>>(pagedStudents));
        }

        /// <summary>
        /// Compone los filtros presentes. Un filtro ausente queda neutralizado por su propia
        /// comparación contra null, que EF resuelve como constante, de modo que el resultado
        /// satisface solo los filtros indicados. Sin ninguno devuelve null, para que el servicio
        /// use la consulta sin predicado en lugar de una condición que siempre se cumple.
        /// </summary>
        private static Expression<Func<StudentEntity, bool>>? BuildPredicate(GetAllStudentParameter filter)
        {
            string? document = Normalize(filter.Document);
            string? lastName = Normalize(filter.LastName);
            string? email = Normalize(filter.Email);

            if (document == null && lastName == null && email == null)
                return null;

            return s => (document == null || s.Document.Contains(document))
                     && (lastName == null || s.LastName.Contains(lastName))
                     && (email == null || s.Email.Contains(email));
        }

        /* Una cadena vacía o en blanco se trata como filtro ausente: filtrar por nada
           devolvería el listado completo, que es lo mismo que no filtrar. */
        private static string? Normalize(string? value) =>
            string.IsNullOrWhiteSpace(value) ? null : value;
    }
}
