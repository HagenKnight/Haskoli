using Haskoli.Domain.Entities;
using Haskoli.Domain.Interfaces.Base;
using Haskoli.Domain.Interfaces.Repository;
using Haskoli.Infrastructure.Persistence.Data;
using Haskoli.Infrastructure.Persistence.Repository.Base;

namespace Haskoli.Infrastructure.Common.Repositories
{
    public class CountryRepository : BaseRepository<Country, int, HaskoliDbContext>, ICountryRepository<HaskoliDbContext>
    {
        public CountryRepository(IDbFactory<HaskoliDbContext> dbFactory) : base(dbFactory) { }
    }
}
