using Haskoli.Domain.DTO;
using FluentValidation;

namespace Haskoli.Application.Features.Country
{
    public  class CreateCountryCommandValidator : AbstractValidator<CreateCountryDTO>
    {
        public CreateCountryCommandValidator()
        {
            RuleFor(u => u.NameEs).Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("{PropertyName} property value is required.")
                .Length(3, 50).WithMessage("{PropertyName} property should be between {MinLength} and {MaxLength} characters in length.");
        }
    }
}
