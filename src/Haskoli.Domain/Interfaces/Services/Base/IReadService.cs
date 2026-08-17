using Microsoft.EntityFrameworkCore;
using Haskoli.Domain.Interfaces.Base;
using Haskoli.Domain.Interfaces.Management;
using System.Linq.Expressions;

namespace Haskoli.Domain.Interfaces.Services.Base
{
    public interface IReadService<T, TQueryDTO, TKey, TEntity, TRepoRead, TContext>
        where T : class
        where TEntity : class, IEntityBase<TKey>
        where TRepoRead : IReadRepository<TEntity, TContext>
        where TContext : DbContext, new()
    {
        Task<TQueryDTO> FindAsync(int id, CancellationToken cancellationToken = default);
        Task<TQueryDTO> GetSingleAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);
 
        Task<IEnumerable<TQueryDTO>> FilterAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default, string fields = null, string orderBy = null);
        Task<IEnumerable<TQueryDTO>> GetAllAsync(CancellationToken cancellationToken = default, string fields = null, string orderBy = null);
        Task<IEnumerable<TQueryDTO>> GetAllAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default, string fields = null, string orderBy = null);

        Task<IEnumerable<TQueryDTO>> GetAllAsyncIncludes(CancellationToken cancellationToken = default, string fields = null, string orderBy = null, params Expression<Func<TEntity, object>>[] includes);


        Task<IEnumerable<TQueryDTO>> GetPagedAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default, string fields = null, string orderBy = null);
        Task<IEnumerable<TQueryDTO>> GetPagedAsync(int pageNumber, int pageSize, Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default, string fields = null, string orderBy = null);
    }
}
