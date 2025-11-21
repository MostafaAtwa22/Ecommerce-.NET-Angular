using System.Linq.Expressions;
using Ecommerce.Core.Entities;

namespace Ecommerce.Core.Spec
{
    public class BaseSepcifications<T> : ISpecifications<T> where T : BaseEntity
    {
        public BaseSepcifications()
        {
        }

        public BaseSepcifications(Expression<Func<T, bool>> criteria)
        {
            Criteria = criteria;
        }

        public Expression<Func<T, bool>> Criteria { get; } = null!;

        public List<Expression<Func<T, object>>> Includes { get; } = new List<Expression<Func<T, object>>>();

        public Expression<Func<T, object>> OrderBy { get; private set; } = null!;

        public Expression<Func<T, object>> OrderByDesc { get; private set; } = null!;

        public int Take { get; private set; }

        public int Skip { get; private set; }

        public bool IsPagingEnabled { get; private set; }

        protected void AddIncludes(Expression<Func<T, object>> includeExpression)
            => Includes.Add(includeExpression);

        protected void AddOrderBy(Expression<Func<T, object>> orderByExpression)
            => OrderBy = orderByExpression;

        protected void AddOrderByDesc(Expression<Func<T, object>> orderByDescExpression)
            => OrderByDesc = orderByDescExpression;

        protected void ApplyPaging(int skip, int take)
        {
            Skip = skip;
            Take = take;
            IsPagingEnabled = true;
        }
    }
}