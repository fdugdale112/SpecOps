namespace SpecOps;

public static class QueryableExtensions
{
    /// <summary>
    /// Filters the query using a specification's expression.
    /// Translates to SQL when used with EF Core.
    /// </summary>
    public static IQueryable<T> WithSpecification<T>(
        this IQueryable<T> query,
        Specification<T> spec) where T : class
        => query.Where(spec.ToExpression());

    /// <summary>
    /// Filters the query using a specification's expression.
    /// A more natural LINQ-style alternative to WithSpecification.
    /// </summary>
    public static IQueryable<T> Where<T>(
        this IQueryable<T> query,
        Specification<T> spec) where T : class
        => query.Where(spec.ToExpression());
}
