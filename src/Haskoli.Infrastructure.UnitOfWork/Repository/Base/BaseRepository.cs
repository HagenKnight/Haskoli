using Haskoli.Domain.Interfaces.Base;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;

namespace Haskoli.Infrastructure.Persistence.Repository.Base
{
    public class BaseRepository<T, TKey, TContext> : IBaseRepository<T, TContext>
        where T : class
        where TContext : DbContext, new()
    {
        private TContext _dataContext;
        private readonly DbSet<T> _dbSet;
        protected IDbFactory<TContext> DbFactory { get; private set; }
        protected TContext DbContext { get => _dataContext ?? (_dataContext = DbFactory.Init()); }
        public BaseRepository(IDbFactory<TContext> dbFactory) { 
            DbFactory = dbFactory; 
            _dbSet = DbContext.Set<T>(); }


        public int GetCount() => _dbSet.Count();
        public int GetCount(Expression<Func<T, bool>> predicate) => _dbSet.Where(predicate).Count();


        public async Task<IEnumerable<T>> AllAsync(string orderBy, CancellationToken cancellationToken = default) =>
            (!string.IsNullOrEmpty(orderBy)) ? await _dbSet.OrderBy(orderBy).ToListAsync(cancellationToken) : await _dbSet.ToListAsync(cancellationToken);

        public async Task<IEnumerable<T>> AllAsync(string orderBy, Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default) =>
            (!string.IsNullOrEmpty(orderBy)) ? await _dbSet.OrderBy(orderBy).ToListAsync(cancellationToken) : await _dbSet.ToListAsync(cancellationToken);

        public async Task<IEnumerable<T>> AllAsyncInclude(string orderBy, CancellationToken cancellationToken = default, params Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> query = _dbSet;
            if (includes != null)
            {
                query = query.IncludeMultiple(includes).AsSingleQuery();
            }
            //query = query.Where(predicate);
            return await query.ToListAsync(cancellationToken);
        }


        public async Task<IEnumerable<T>> FilterAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default, string orderBy = null) =>
            (!string.IsNullOrEmpty(orderBy)) ? await _dbSet.Where(predicate).OrderBy(orderBy).ToListAsync(cancellationToken) : await _dbSet.Where(predicate).ToListAsync(cancellationToken);


        public async Task<T> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
          await _dbSet.FindAsync(new object[] { id }, cancellationToken);


        public async Task<T> FilterSingleAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default) =>
            await _dbSet.SingleOrDefaultAsync(predicate, cancellationToken);


        public async Task<T> FilterSingleAsync(Expression<Func<T, bool>> predicate, string entityToInclude = null, CancellationToken cancellationToken = default)
        {
            if (!string.IsNullOrEmpty(entityToInclude))
                return await _dbSet.Include(entityToInclude).SingleOrDefaultAsync(predicate, cancellationToken);
            else
                return await _dbSet.SingleOrDefaultAsync(predicate, cancellationToken);
        }

        public async Task<IEnumerable<T>> GetPagedAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default, string orderBy = null) =>
            (!string.IsNullOrEmpty(orderBy)) ? await _dbSet.OrderBy(orderBy).Skip((pageNumber - 1) * pageSize).Take(pageSize).AsNoTracking().ToListAsync(cancellationToken) :
                                        await _dbSet.Skip((pageNumber - 1) * pageSize).Take(pageSize).AsNoTracking().ToListAsync(cancellationToken);
        public async Task<IEnumerable<T>> GetPagedAsync(int pageNumber, int pageSize, Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default, string orderBy = null) =>
            (!string.IsNullOrEmpty(orderBy)) ? await _dbSet.OrderBy(orderBy).Where(predicate).Skip((pageNumber - 1) * pageSize).Take(pageSize).AsNoTracking().ToListAsync(cancellationToken) :
                                                await _dbSet.Where(predicate).Skip((pageNumber - 1) * pageSize).Take(pageSize).AsNoTracking().ToListAsync(cancellationToken);


        public async Task AddAsync(T entity, CancellationToken cancellationToken = default) =>
          await _dbSet.AddAsync(entity, cancellationToken);

        public async Task AddRangeAsync(IEnumerable<T> EntityList, CancellationToken cancellationToken = default) =>
            await _dbSet.AddRangeAsync(EntityList, cancellationToken);

        public void Update(T entity) => _dbSet.Update(entity);

        public void Delete(T entity) => _dbSet.Remove(entity);

        public void DeleteRange(IEnumerable<T> entity, Expression<Func<T, bool>> predicate)
        {
            _dbSet.Where(predicate).ExecuteDelete() ;
        }


    }

    public static class RepositoryExtensions
    {
        public static IQueryable<T> IncludeMultiple<T>(this IQueryable<T> query,
                                                       params Expression<Func<T, object>>[] includes)
                                                       where T : class
        {
            if (includes != null)
            {
                query = includes.Aggregate(query,
                             (current, include) => current.Include(include));
            }
            return query;
        }
    }
}
