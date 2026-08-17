using Haskoli.Domain.DTO;
using Haskoli.Domain.Interfaces.Services;
using Haskoli.Domain.Wrappers;
using MediatR;

namespace Haskoli.Application.Features.Country
{
    public class DeleteCountryCommandHandler : IRequestHandler<DeleteCountryDTO, ApiResponse<DeleteCountryDTO>>
    {
        private readonly ICountryService _countryService;

        public DeleteCountryCommandHandler(ICountryService entityService) =>
            _countryService = entityService;

        public Task<ApiResponse<DeleteCountryDTO>> Handle(DeleteCountryDTO request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
