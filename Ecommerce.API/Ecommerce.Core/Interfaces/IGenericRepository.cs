using System.Linq.Expressions;
using Ecommerce.Core.Entities;
using Ecommerce.Core.Spec;

namespace Ecommerce.Core.Interfaces
{
    public interface IGenericRepository<T> where T : BaseEntity
    {
        Task<T?> GetByIdAsync(int id);
        Task<T?> GetWithSpecAsync(ISpecifications<T> specifications);
        Task<T?> FindAsync(Expression<Func<T, bool>> query);
        Task<IReadOnlyList<T>> GetAllAsync();
        Task<IReadOnlyList<T>> GetAllWithSpecAsync(ISpecifications<T> specifications);
        Task<IReadOnlyList<T>> FindAllAsync(Expression<Func<T, bool>> query);
        Task Create(T entity);
        Task AddRange(IEnumerable<T> entities);
        void Update(T entity);
        void UpdateRanage(IEnumerable<T> entities);
        void Delete(T entity);
        void DeleteRange(IEnumerable<T> entities);
        Task<int> CountAsync();
        Task<int> CountAsync(ISpecifications<T> specifications);
    }
}