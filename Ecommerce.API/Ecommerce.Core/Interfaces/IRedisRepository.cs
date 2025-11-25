namespace Ecommerce.Core.Interfaces
{
    public interface IRedisRepository<T>
    {
        Task<T?> GetAsync(string id);
        Task<T?> UpdateOrCreateAsync(string id, T entity);
        Task<bool> DeleteAsync(string id);
    }
}