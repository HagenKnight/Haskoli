using Haskoli.Domain.DTO.Base;
using Haskoli.Domain.Wrappers;
using MediatR;

namespace Haskoli.Domain.DTO
{
    public class CreateStudentDTO : CommandDTO, IRequest<ApiResponse<StudentDTO>>
    {
        public string Document { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }
}
