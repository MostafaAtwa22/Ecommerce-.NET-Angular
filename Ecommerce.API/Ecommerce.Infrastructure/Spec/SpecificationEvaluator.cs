using Ecommerce.Core.Entities;
using Ecommerce.Core.Spec;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Infrastructure.Spec
{
    public static class SpecificationEvaluator<T> where T : BaseEntity
    {
        public static IQueryable<T> GetQuery (IQueryable<T> inputQuery, ISpecifications<T> specifications)
        {
            var query = inputQuery;

            if (specifications.Criteria is not null)
                query = query.Where(specifications.Criteria);

            query = specifications.Includes.Aggregate(query, (cur, include) => cur.Include(include));

            return query;
        }
    }
}