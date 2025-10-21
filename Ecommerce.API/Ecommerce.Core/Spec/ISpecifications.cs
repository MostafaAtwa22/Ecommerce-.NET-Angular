using System.Linq.Expressions;
using Ecommerce.Core.Entities;

namespace Ecommerce.Core.Spec
{
    public interface ISpecifications<T> where T : BaseEntity
    {
        public Expression<Func<T, bool>> Criteria { get; }
        public List<Expression<Func<T, object>>> Includes { get; }
    }
}