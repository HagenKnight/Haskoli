using FluentValidation;
using Haskoli.Domain.DTO;

namespace Haskoli.Application.Features.Student
{
    public class DeleteStudentCommandValidator : AbstractValidator<DeleteStudentDTO>
    {
        public DeleteStudentCommandValidator()
        {
            RuleFor(s => s.Id).IdRules();
        }
    }
}
