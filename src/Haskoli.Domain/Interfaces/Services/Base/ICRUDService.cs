using Haskoli.Domain.Interfaces.Base;
using Haskoli.Domain.Interfaces.Management;
using Microsoft.EntityFrameworkCore;

namespace Haskoli.Domain.Interfaces.Services.Base
{
    public interface ICRUDService<T, TQueryDTO, TCommandDTO, TKey, TEntity, TRepoAll, TContext> :
        IReadService<T, TQueryDTO, TKey, TEntity, TRepoAll, TContext>
     where T : class
     where TEntity : class, IEntityBase<TKey>
     where TRepoAll : IBaseRepository<TEntity, TContext>
     where TContext : DbContext, new()
    {
        Task<TQueryDTO> UpdateAsync(TCommandDTO objDTO, CancellationToken cancellationToken = default);
        Task<TQueryDTO> InsertAsync(TCommandDTO objDTO, CancellationToken cancellationToken = default);
        Task<TQueryDTO> DeleteAsync(TCommandDTO objDTO, bool autoSave = true, CancellationToken cancellationToken = default);
        Task<IEnumerable<TQueryDTO>> BulkInsertAsync(IEnumerable<TCommandDTO> objDTO, CancellationToken cancellationToken = default);
        Task BulkDeleteAsync(IEnumerable<TCommandDTO> objDTO, CancellationToken cancellationToken = default);
    }
}
