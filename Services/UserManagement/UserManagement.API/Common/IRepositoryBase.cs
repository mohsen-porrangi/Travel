using System.Linq.Expressions;

namespace UserManagement.API.Common
{
    public interface IRepositoryBase<T, in TKey>
    {
        Task<T?> GetByIdAsync(TKey id, CancellationToken cancellationToken,bool track = false);
        Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken, bool track = false);
        Task<IEnumerable<T>> GetByConditionAsync(Expression<Func<T, bool>> predicate, bool track = false);
        Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, bool track = false);

        Task AddAsync(T entity);
        Task AddRangeAsync(IEnumerable<T> entities);
        void Update(T entity);
        Task UpdateAsync(T entity);
        Task DeleteAsync(TKey id, CancellationToken cancellationToken);
        void DeleteRange(IEnumerable<T> entities);
    }
}
