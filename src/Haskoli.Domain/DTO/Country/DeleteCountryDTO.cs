using Haskoli.Domain.DTO.Base;
using Haskoli.Domain.Wrappers;
using MediatR;

namespace Haskoli.Domain.DTO
{
    public class DeleteCountryDTO : CommandDTO, IRequest<ApiResponse<DeleteCountryDTO>>
    {
        public DateTime DeleteDate { get; set; }
        public bool AutoSave { get; set; }
    }

}
