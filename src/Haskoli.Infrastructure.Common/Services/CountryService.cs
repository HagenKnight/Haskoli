using AutoMapper;
using Haskoli.Domain.DTO;
using Haskoli.Domain.DTO.Base;
using Haskoli.Domain.Entities;
using Haskoli.Domain.Interfaces.Base;
using Haskoli.Domain.Interfaces.Management;
using Haskoli.Domain.Interfaces.Repository;
using Haskoli.Domain.Interfaces.Services;
using Haskoli.Infrastructure.Persistence.Data;
using Haskoli.Infrastructure.Persistence.Services.Base;

namespace Haskoli.Infrastructure.Common.Services
{
    public class CountryService : CRUDService<Country, CountryDTO, CommandDTO, int,
                                  Country, ICountryRepository<HaskoliDbContext>, HaskoliDbContext>,
                                  ICountryService
    {
        private readonly IDataShapeHelper<CountryDTO> _dataShaperHelper;

        public CountryService(ICountryRepository<HaskoliDbContext> repository,
            IUnitOfWork<HaskoliDbContext> unitOfWork,
            IMapper mapper,
            IDataShapeHelper<CountryDTO> dataShapeHelper
            ) : base(repository,
                unitOfWork,
                mapper)
        {
            _dataShaperHelper = dataShapeHelper;
        }

        public async Task<IEnumerable<CountryDTO>> GetCountries(CancellationToken cancellationToken = default) =>
            await GetAllAsync(cancellationToken);

        public async Task<CountryDTO> FindCountry(int id, CancellationToken cancellationToken = default) =>
            await FindAsync(id, cancellationToken);


        //public async Task<IEnumerable<ShapedEntityDTO>> GetCountries(CancellationToken cancellationToken = default, string fields = null, string orderBy = null) =>
        //    await _dataShaperHelper.ShapeDataAsync(await GetAllAsync(cancellationToken, fields, orderBy), fields);


        //public async Task<IEnumerable<ShapedEntityDTO>> GetPagedCountries(int pageNumber, int pageSize, CancellationToken cancellationToken = default, Expression<Func<Country, bool>> predicate = null, string fields = null, string orderBy = null) =>
        //    (predicate == null) ? await _dataShaperHelper.ShapeDataAsync(await GetPagedAsync(pageNumber, pageSize, cancellationToken, fields, orderBy), fields) :
        //                await _dataShaperHelper.ShapeDataAsync(await GetPagedAsync(pageNumber, pageSize, predicate, cancellationToken, fields, orderBy), fields);


    }
}
