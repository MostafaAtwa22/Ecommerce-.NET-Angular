using System.Linq.Expressions;
using Ecommerce.Core.Entities;

namespace Ecommerce.Core.Spec
{
    public class SpecificationBuilder<T> : BaseSepcifications<T> where T : BaseEntity
    {
        public SpecificationBuilder()
        {
        }

        public SpecificationBuilder(Expression<Func<T, bool>> criteria)
            : base(criteria)
        {
        }

        public SpecificationBuilder<T> Where(Expression<Func<T, bool>> criteria)
        {
            Criteria = criteria;
            return this;
        }

        public SpecificationBuilder<T> And(Expression<Func<T, bool>> criteria)
        {
            if (Criteria is null)
            {
                Criteria = criteria;
                return this;
            }

            var param = Expression.Parameter(typeof(T));

            var leftVisitor = new ReplaceExpressionVisitor(criteria.Parameters[0], param);
            var left = leftVisitor.Visit(criteria.Body)!;

            var rightVisitor = new ReplaceExpressionVisitor(Criteria.Parameters[0], param);
            var right = rightVisitor.Visit(Criteria.Body)!;

            Criteria = Expression.Lambda<Func<T, bool>>(
                Expression.AndAlso(left, right),
                param);

            return this;
        }

        public SpecificationBuilder<T> Include(Expression<Func<T, object>> includeExpression)
        {
            AddIncludes(includeExpression);
            return this;
        }

        public SpecificationBuilder<T> OrderByAsc(Expression<Func<T, object>> orderByExpression)
        {
            AddOrderBy(orderByExpression);
            return this;
        }

        public new SpecificationBuilder<T> OrderByDesc(Expression<Func<T, object>> orderByDescExpression)
        {
            AddOrderByDesc(orderByDescExpression);
            return this;
        }

        public SpecificationBuilder<T> Paginate(int pageIndex, int pageSize)
        {
            ApplyPaging((pageIndex - 1) * pageSize, pageSize);
            return this;
        }

        public SpecificationBuilder<T> WithTracking()
        {
            DisableTracking();
            return this;
        }

        public SpecificationBuilder<T> WithoutSplitQuery()
        {
            DisableSplitQuery();
            return this;
        }

        private sealed class ReplaceExpressionVisitor : ExpressionVisitor
        {
            private readonly Expression _oldValue;
            private readonly Expression _newValue;

            public ReplaceExpressionVisitor(Expression oldValue, Expression newValue)
            {
                _oldValue = oldValue;
                _newValue = newValue;
            }

            public override Expression? Visit(Expression? node)
            {
                if (node == _oldValue)
                    return _newValue;
                return base.Visit(node);
            }
        }
    }
}

