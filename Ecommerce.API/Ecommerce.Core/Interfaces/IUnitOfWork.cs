using Microsoft.EntityFrameworkCore.Storage;

namespace Ecommerce.Core.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IGenericRepository<T> Repository<T>() where T : BaseEntity;
        Task<IDbContextTransaction> BeginTransactionAsync();
        Task<int> Complete();
    }
}
