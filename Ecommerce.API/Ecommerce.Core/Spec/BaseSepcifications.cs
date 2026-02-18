
namespace Ecommerce.Core.Spec
{
    public class BaseSepcifications<T> : ISpecifications<T> where T : BaseEntity
    {
        public BaseSepcifications()
        { }

        public BaseSepcifications(Expression<Func<T, bool>> criteria)
        {
            Criteria = criteria;
        }

        public Expression<Func<T, bool>>? Criteria { get; protected set; }

        public List<Expression<Func<T, object>>> Includes { get; } = new List<Expression<Func<T, object>>>();

        public Expression<Func<T, object>>? OrderBy { get; protected set; }

        public Expression<Func<T, object>>? OrderByDesc { get; protected set; }

        public int Take { get; protected set; }

        public int Skip { get; protected set; }

        public bool IsPagingEnabled { get; protected set; }

        public bool AsNoTracking { get; protected set; } = true;

        public bool UseSplitQuery { get; protected set; } = true;

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

        protected void DisableTracking()
        {
            AsNoTracking = false;
        }

        protected void DisableSplitQuery()
        {
            UseSplitQuery = false;
        }
    }
}
