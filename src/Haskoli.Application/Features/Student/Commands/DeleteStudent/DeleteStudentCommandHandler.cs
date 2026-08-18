using Haskoli.Application.Constants;
using Haskoli.Domain.DTO;
using Haskoli.Domain.Interfaces.Services;
using Haskoli.Domain.Wrappers;
using MediatR;

namespace Haskoli.Application.Features.Student
{
    public class DeleteStudentCommandHandler : IRequestHandler<DeleteStudentDTO, ApiResponse<StudentDTO>>
    {
        private readonly IStudentService _studentService;
        public DeleteStudentCommandHandler(IStudentService studentService) => _studentService = studentService;

        public async Task<ApiResponse<StudentDTO>> Handle(DeleteStudentDTO request, CancellationToken cancellationToken)
        {
            /* La búsqueda descarta a los ya eliminados, de modo que eliminar dos veces el mismo
               estudiante falla como inexistente en lugar de volver a sellar la auditoría. */
            await StudentLookup.FindOrThrowAsync(_studentService, request.Id, cancellationToken);

            StudentDTO deleted = await _studentService.DeleteStudent(request, cancellationToken);

            return new ApiResponse<StudentDTO>(deleted, StudentMessages.Deleted);
        }
    }
}
