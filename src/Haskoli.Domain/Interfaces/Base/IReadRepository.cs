using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Haskoli.Domain.Interfaces.Base
{
    public interface IReadRepository<T, TContext>
        where T : class
        where TContext : DbContext, new()
    {
        int GetCount();
        int GetCount(Expression<Func<T, bool>> predicate);

        Task<IEnumerable<T>> AllAsync(string orderBy, CancellationToken cancellationToken = default);
        Task<IEnumerable<T>> AllAsync(string orderBy, Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
        Task<IEnumerable<T>> AllAsyncInclude(string orderBy, CancellationToken cancellationToken = default, params Expression<Func<T, object>>[] includes);

        Task<T> GetByIdAsync(int id, CancellationToken cancellationToken = default);

        Task<T> FilterSingleAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
        Task<T> FilterSingleAsync(Expression<Func<T, bool>> predicate, string entityToInclude = null, CancellationToken cancellationToken = default);

        Task<IEnumerable<T>> FilterAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default, string orderBy = null);


        Task<IEnumerable<T>> GetPagedAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default, string orderBy = null);
        Task<IEnumerable<T>> GetPagedAsync(int pageNumber, int pageSize, Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default, string orderBy = null);
    }
}
