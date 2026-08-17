using Haskoli.Domain.DTO;
using Haskoli.Domain.Interfaces.Services;
using Haskoli.Domain.Wrappers;
using MediatR;

namespace Haskoli.Application.Features.Country
{
    public class UpdateCountryCommandHandler : IRequestHandler<UpdateCountryDTO, ApiResponse<UpdateCountryDTO>>
    {
        private readonly ICountryService _countryService;
        public UpdateCountryCommandHandler(ICountryService entityService) => _countryService = entityService;

        public Task<ApiResponse<UpdateCountryDTO>> Handle(UpdateCountryDTO request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
