using System.Linq.Expressions;

namespace SpecOps;

internal sealed class NotSpecification<T>(Specification<T> spec) : Specification<T>
    where T : class
{
    public override Expression<Func<T, bool>> ToExpression()
    {
        var expr = spec.ToExpression();
        var parameter = Expression.Parameter(typeof(T), "e");

        var body = Expression.Not(Expression.Invoke(expr, parameter));

        return Expression.Lambda<Func<T, bool>>(body, parameter);
    }
}
