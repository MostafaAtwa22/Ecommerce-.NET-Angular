using Ecommerce.Core.Entities;
using Microsoft.EntityFrameworkCore.Storage;

namespace Ecommerce.Core.Interfaces
{
    public interface IUnitOfWork
    {
        IGenericRepository<T> Repository<T>() where T : BaseEntity;

        Task<int> Complete();
        Task<IDbContextTransaction> BeginTransactionAsync();
    }
}