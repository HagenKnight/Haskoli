using Haskoli.Domain.DTO;
using Haskoli.Domain.Interfaces.Services;
using MediatR;

namespace Haskoli.Application.Features.Country
{
    public class GetCountryHandler : IRequestHandler<GetCountryQuery, CountryDTO>
    {
        private readonly ICountryService _countryService;
        public GetCountryHandler(ICountryService countryService) => _countryService = countryService;

        public async Task<CountryDTO> Handle(GetCountryQuery request, CancellationToken cancellationToken) =>
             await _countryService.FindCountry(request.Id, cancellationToken);

    }
}
