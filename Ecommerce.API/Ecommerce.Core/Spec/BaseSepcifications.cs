using System.Linq.Expressions;
using Ecommerce.Core.Entities;

namespace Ecommerce.Core.Spec
{
    public class BaseSepcifications<T> : ISpecifications<T> where T : BaseEntity
    {
        public BaseSepcifications()
        {
            this.Criteria = null!;
            Includes = new List<Expression<Func<T, object>>>();
        }

        public BaseSepcifications(Expression<Func<T, bool>> criteria)
        {
            this.Criteria = criteria;
            Includes = new List<Expression<Func<T, object>>>();
        }

        public Expression<Func<T, bool>> Criteria { get; }

        public List<Expression<Func<T, object>>> Includes { get; }

        protected void AddIncludes(Expression<Func<T, object>> includeExpression)
            => Includes.Add(includeExpression);
    }
}