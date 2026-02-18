
namespace Ecommerce.Infrastructure.Spec
{
    public static class SpecificationEvaluator<T> where T : BaseEntity
    {
        public static IQueryable<T> GetQuery (IQueryable<T> inputQuery, ISpecifications<T> specifications)
        {
            var query = inputQuery;

            if (specifications.AsNoTracking)
                query = query.AsNoTracking();

            if (specifications.UseSplitQuery)
                query = query.AsSplitQuery();

            if (specifications.Criteria is not null)
                query = query.Where(specifications.Criteria);

            if (specifications.OrderBy is not null)
                query = query.OrderBy(specifications.OrderBy);

            if (specifications.OrderByDesc is not null)
                query = query.OrderByDescending(specifications.OrderByDesc);

            if (specifications.IsPagingEnabled)
                query = query
                    .Skip(specifications.Skip)
                    .Take(specifications.Take);

            query = specifications.Includes.Aggregate(query, (cur, include) => cur.Include(include));

            return query;
        }
    }
}
