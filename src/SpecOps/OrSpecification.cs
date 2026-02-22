using System.Linq.Expressions;

namespace SpecOps;

internal sealed class OrSpecification<T>(Specification<T> left, Specification<T> right) : Specification<T>
    where T : class
{
    public override Expression<Func<T, bool>> ToExpression()
    {
        var leftExpr = left.ToExpression();
        var rightExpr = right.ToExpression();
        var parameter = Expression.Parameter(typeof(T), "e");

        var body = Expression.OrElse(
            Expression.Invoke(leftExpr, parameter),
            Expression.Invoke(rightExpr, parameter));

        return Expression.Lambda<Func<T, bool>>(body, parameter);
    }
}
