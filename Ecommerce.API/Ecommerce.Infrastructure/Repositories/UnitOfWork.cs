using System.Collections.Concurrent;
using Microsoft.EntityFrameworkCore.Storage;

namespace Ecommerce.Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private readonly ConcurrentDictionary<string,object> _repositories = new();

        public UnitOfWork(ApplicationDbContext context)
            => _context = context;

        public IGenericRepository<T> Repository<T>() where T : BaseEntity
        {
            var key = typeof(T).Name;
            return (IGenericRepository<T>) _repositories.GetOrAdd(key, _ => new GenericRepository<T>(_context));
        }

        public async Task<IDbContextTransaction> BeginTransactionAsync()
            => await _context.Database.BeginTransactionAsync();

        public async Task<int> Complete()
            => await _context.SaveChangesAsync();

        public void Dispose()   
            => _context.Dispose();
    }
}
