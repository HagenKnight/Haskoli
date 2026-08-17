using Haskoli.Domain.DTO;
using MediatR;

namespace Haskoli.Application.Features.Country
{

    public class GetAllCountryQuery : IRequest<IEnumerable<CountryDTO>> { }

    public class GetCountryQuery : IRequest<CountryDTO>
    {
        public int Id { get; set; }
        public GetCountryQuery(int id) => Id = id;
    }
}
