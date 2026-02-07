using System.Linq.Expressions;
using Ecommerce.Core.Entities;
using Ecommerce.Core.Interfaces;
using Ecommerce.Core.Spec;
using Ecommerce.Infrastructure.Data;
using Ecommerce.Infrastructure.Spec;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Infrastructure.Repositories
{
    public class GenericRepository<T> : IGenericRepository<T> where T : BaseEntity
    {
        private readonly ApplicationDbContext _context;

        public GenericRepository(ApplicationDbContext context)
            => _context = context;

        public async Task<T?> GetByIdAsync(int id)
            => await _context.Set<T>().FindAsync(id);
        public async Task<T?> GetWithSpecAsync(ISpecifications<T> specifications)
            => await ApplySpecification(specifications)
                .FirstOrDefaultAsync();

        public async Task<T?> FindAsync(Expression<Func<T, bool>> query)
            => await _context.Set<T>().SingleOrDefaultAsync(query);

        public async Task<IReadOnlyList<T>> GetAllAsync()
            => await _context.Set<T>().ToListAsync();

        public async Task<IReadOnlyList<T>> GetAllWithSpecAsync(ISpecifications<T> specifications)
            => await ApplySpecification(specifications).ToListAsync();

        public async Task<IReadOnlyList<T>> FindAllAsync(Expression<Func<T, bool>> query)
            => await _context.Set<T>().Where(query).ToListAsync();

        public async Task Create(T entity)
            => await _context.Set<T>().AddAsync(entity);

        public async Task AddRange(IEnumerable<T> entities)
            => await _context.Set<T>().AddRangeAsync(entities);
        public void Update(T entity)
            => _context.Set<T>().Update(entity);

        public void UpdateRanage(IEnumerable<T> entities)
            => _context.Set<T>().UpdateRange(entities);

        public void Delete(T entity)
            => _context.Set<T>().Remove(entity);

        public void DeleteRange(IEnumerable<T> entities)
            => _context.Set<T>().RemoveRange(entities);

        public async Task<int> CountAsync()
            => await _context.Set<T>().CountAsync();

        public async Task<int> CountAsync(ISpecifications<T> specifications)
            => await ApplySpecification(specifications).CountAsync();

        public IQueryable<T> GetAllQueryable()
            => _context.Set<T>().AsQueryable();

        private IQueryable<T> ApplySpecification(ISpecifications<T> specifications)
            => SpecificationEvaluator<T>.GetQuery(_context.Set<T>().AsQueryable(), specifications);
    }
}