using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Haskoli.Domain.Interfaces.Base
{
    public interface IBaseRepository<T, TContext> : IReadRepository<T, TContext>
      where T : class
      where TContext : DbContext, new()
    {
        Task AddAsync(T Entity, CancellationToken cancellationToken = default);
        Task AddRangeAsync(IEnumerable<T> EntityList, CancellationToken cancellationToken = default);
        void Update(T Entity);
        void Delete(T entity);
        void DeleteRange(IEnumerable<T> entityList, Expression<Func<T, bool>> predicate);
    }
}
