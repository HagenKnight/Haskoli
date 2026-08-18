using Haskoli.Application.Constants;
using Haskoli.Domain.DTO;
using Haskoli.Domain.Exceptions.Core;
using Haskoli.Domain.Interfaces.Services;
using Haskoli.Domain.Wrappers;
using MediatR;

namespace Haskoli.Application.Features.Student
{
    public class UpdateStudentCommandHandler : IRequestHandler<UpdateStudentDTO, ApiResponse<StudentDTO>>
    {
        private readonly IStudentService _studentService;
        public UpdateStudentCommandHandler(IStudentService studentService) => _studentService = studentService;

        public async Task<ApiResponse<StudentDTO>> Handle(UpdateStudentDTO request, CancellationToken cancellationToken)
        {
            await StudentLookup.FindOrThrowAsync(_studentService, request.Id, cancellationToken);

            /* Se excluye el propio Id para que reenviar el documento y el email que el
               estudiante ya tenía sea una actualización válida. */
            if (await _studentService.ExistsByDocument(request.Document, request.Id, cancellationToken))
                throw new EntityAlreadyExistException(StudentMessages.DocumentAlreadyRegistered(request.Document));

            if (await _studentService.ExistsByEmail(request.Email, request.Id, cancellationToken))
                throw new EntityAlreadyExistException(StudentMessages.EmailAlreadyRegistered(request.Email));

            StudentDTO updated = await _studentService.UpdateStudent(request, cancellationToken);

            return new ApiResponse<StudentDTO>(updated, StudentMessages.Updated);
        }
    }
}
