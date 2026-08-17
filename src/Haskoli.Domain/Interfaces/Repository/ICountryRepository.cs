using Haskoli.Domain.Entities;
using Haskoli.Domain.Interfaces.Base;
using Microsoft.EntityFrameworkCore;

namespace Haskoli.Domain.Interfaces.Repository
{
    public interface ICountryRepository<TContext> : IBaseRepository<Country, TContext> where TContext : DbContext, new()
    {

    }
}
