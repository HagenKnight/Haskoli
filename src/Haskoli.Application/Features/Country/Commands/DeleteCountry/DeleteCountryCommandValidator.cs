using Haskoli.Domain.DTO;
using FluentValidation;

namespace Haskoli.Application.Features.Country
{
    public class DeleteCountryCommandValidator : AbstractValidator<DeleteCountryDTO>
    {
        public DeleteCountryCommandValidator()
        {
            RuleFor(u => u.Id).Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("You must chose a {PropertyName}.")
                .GreaterThan(0).WithMessage("The {PropertyName} index should be greater than 0.");
        }
    }
}
