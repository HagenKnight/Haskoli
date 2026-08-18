using FluentValidation;
using Haskoli.Domain.DTO;

namespace Haskoli.Application.Features.Student
{
    public class CreateStudentCommandValidator : AbstractValidator<CreateStudentDTO>
    {
        public CreateStudentCommandValidator()
        {
            RuleFor(s => s.Document).DocumentRules();
            RuleFor(s => s.FirstName).FirstNameRules();
            RuleFor(s => s.LastName).LastNameRules();
            RuleFor(s => s.Email).EmailRules();
        }
    }
}
