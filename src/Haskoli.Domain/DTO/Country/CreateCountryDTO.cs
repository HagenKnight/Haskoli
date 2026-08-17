using Haskoli.Domain.DTO.Base;
using Haskoli.Domain.Wrappers;
using MediatR;

namespace Haskoli.Domain.DTO
{
    public class CreateCountryDTO : CommandDTO, IRequest<ApiResponse<CreateCountryDTO>>
    {
        public string NameEs { get; set; } = string.Empty;
        public string NameEn { get; set; } = string.Empty;
        public string ISO2 { get; set; } = string.Empty;
        public string ISO3 { get; set; } = string.Empty;
        public DateTime CreationDate { get; set; }
    }

}
