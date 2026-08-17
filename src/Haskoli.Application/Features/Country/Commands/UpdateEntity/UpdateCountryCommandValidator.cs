using Haskoli.Domain.DTO;
using FluentValidation;

namespace Haskoli.Application.Features.Country
{
    public  class UpdateCountryCommandValidator : AbstractValidator<UpdateCountryDTO>
    {
        public UpdateCountryCommandValidator()
        {
            RuleFor(u => u.Id).Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("{PropertyName} property value is required.");
        }
    }
}
