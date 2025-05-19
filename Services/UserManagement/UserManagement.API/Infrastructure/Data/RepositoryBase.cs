using BuildingBlocks.Contracts;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using UserManagement.API.Infrastructure.Data.Models;

namespace UserManagement.API.Infrastructure.Data
{
    public abstract class RepositoryBase<T, TKey> : IRepositoryBase<T, TKey> where T : BaseEntity<TKey>
    {
        protected readonly AppDbContext _db;
        protected readonly DbSet<T> _dbSet;

        protected RepositoryBase(AppDbContext db)
        {
            _db = db;
            _dbSet = db.Set<T>();
        }

        public virtual async Task<T?> GetByIdAsync(TKey id, CancellationToken cancellationToken, bool track = false)
        {
            var query = _dbSet.AsQueryable();
            if (!track)
                query = query.AsNoTracking();
            return await query.FirstOrDefaultAsync(x => x.Id!.Equals(id));
        }

        public virtual async Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken ,bool track = false)
        {
            var query = _dbSet.AsQueryable();
            if (!track)
                query = query.AsNoTracking();
            return await query.ToListAsync();
        }

        public virtual async Task<IEnumerable<T>> GetByConditionAsync(Expression<Func<T, bool>> predicate, bool track = false)
        {
            var query = _dbSet.Where(predicate);
            if (!track)
                query = query.AsNoTracking();
            return await query.ToListAsync();
        }

        public virtual async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, bool track = false)
        {
            var query = _dbSet.Where(predicate);
            if (!track)
                query = query.AsNoTracking();
            return await query.FirstOrDefaultAsync();
        }

        public virtual Task AddAsync(T entity)
        {
            return _dbSet.AddAsync(entity).AsTask();
        }

        public virtual Task AddRangeAsync(IEnumerable<T> entities)
        {
            return _dbSet.AddRangeAsync(entities);
        }

        public virtual void Update(T entity)
        {
            _dbSet.Update(entity);
        }

        public virtual Task UpdateAsync(T entity)
        {
            _dbSet.Update(entity);
            return Task.CompletedTask; 
        }

        public virtual async Task DeleteAsync(TKey id, CancellationToken cancellationToken)
        {
            var entity = await GetByIdAsync(id, cancellationToken, track: true);
            if (entity != null)
                _dbSet.Remove(entity);
        }

        public virtual void DeleteRange(IEnumerable<T> entities)
        {
            _dbSet.RemoveRange(entities);
        }
    }

}
