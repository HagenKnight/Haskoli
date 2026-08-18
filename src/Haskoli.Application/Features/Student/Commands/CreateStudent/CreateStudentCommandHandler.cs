using Haskoli.Application.Constants;
using Haskoli.Domain.DTO;
using Haskoli.Domain.Exceptions.Core;
using Haskoli.Domain.Interfaces.Services;
using Haskoli.Domain.Wrappers;
using MediatR;

namespace Haskoli.Application.Features.Student
{
    public class CreateStudentCommandHandler : IRequestHandler<CreateStudentDTO, ApiResponse<StudentDTO>>
    {
        private readonly IStudentService _studentService;
        public CreateStudentCommandHandler(IStudentService studentService) => _studentService = studentService;

        public async Task<ApiResponse<StudentDTO>> Handle(CreateStudentDTO request, CancellationToken cancellationToken)
        {
            /* El documento se comprueba antes que el email para que un conflicto simultáneo
               reporte el documento, como fija la especificación. Ambas comprobaciones alcanzan
               a los estudiantes eliminados, porque los índices únicos también los abarcan. */
            if (await _studentService.ExistsByDocument(request.Document, null, cancellationToken))
                throw new EntityAlreadyExistException(StudentMessages.DocumentAlreadyRegistered(request.Document));

            if (await _studentService.ExistsByEmail(request.Email, null, cancellationToken))
                throw new EntityAlreadyExistException(StudentMessages.EmailAlreadyRegistered(request.Email));

            StudentDTO created = await _studentService.CreateStudent(request, cancellationToken);

            return new ApiResponse<StudentDTO>(created, StudentMessages.Created);
        }
    }
}
