using Haskoli.Domain.Custom;
using Haskoli.Domain.DTO;
using Haskoli.Domain.Wrappers;
using MediatR;

namespace Haskoli.Application.Features.Student
{
    public class GetAllStudentQuery : IRequest<ApiResponse<MetaData<StudentDTO>>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? OrderBy { get; set; }
        public string? Route { get; set; }

        /* Filtros opcionales e independientes; se combinan con AND cuando llega más de uno. */
        public string? Document { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
    }

    public class GetStudentQuery : IRequest<ApiResponse<StudentDTO>>
    {
        public int Id { get; set; }
        public GetStudentQuery(int id) => Id = id;
    }
}
