using Haskoli.Domain.DTO;
using Haskoli.Domain.Interfaces.Services;
using Haskoli.Domain.Wrappers;
using MediatR;

namespace Haskoli.Application.Features.Student
{
    public class GetStudentHandler : IRequestHandler<GetStudentQuery, ApiResponse<StudentDTO>>
    {
        private readonly IStudentService _studentService;
        public GetStudentHandler(IStudentService studentService) => _studentService = studentService;

        public async Task<ApiResponse<StudentDTO>> Handle(GetStudentQuery request, CancellationToken cancellationToken) =>
            new ApiResponse<StudentDTO>(await StudentLookup.FindOrThrowAsync(_studentService, request.Id, cancellationToken));
    }
}
