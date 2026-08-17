using Haskoli.Domain.DTO;
using Haskoli.Domain.Interfaces.Services;
using Haskoli.Domain.Wrappers;
using MediatR;

namespace Haskoli.Application.Features.Country
{
    public class CreateCountryCommandHandler : IRequestHandler<CreateCountryDTO, ApiResponse<CreateCountryDTO>>
    {
        private readonly ICountryService _countryService;
        public CreateCountryCommandHandler(ICountryService entityService) => _countryService = entityService;

        public Task<ApiResponse<CreateCountryDTO>> Handle(CreateCountryDTO request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
