using FluentValidation;
using Haskoli.Domain.DTO;

namespace Haskoli.Application.Features.Student
{
    public class UpdateStudentCommandValidator : AbstractValidator<UpdateStudentDTO>
    {
        public UpdateStudentCommandValidator()
        {
            RuleFor(s => s.Id).IdRules();
            RuleFor(s => s.Document).DocumentRules();
            RuleFor(s => s.FirstName).FirstNameRules();
            RuleFor(s => s.LastName).LastNameRules();
            RuleFor(s => s.Email).EmailRules();
        }
    }
}
