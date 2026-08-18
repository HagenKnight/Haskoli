using Haskoli.Domain.DTO.Base;
using Haskoli.Domain.Wrappers;
using MediatR;

namespace Haskoli.Domain.DTO
{
    public class DeleteStudentDTO : CommandDTO, IRequest<ApiResponse<StudentDTO>>
    {
    }
}
