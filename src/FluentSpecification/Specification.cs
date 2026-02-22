using System.Linq.Expressions;

namespace FluentSpecification;

/// <summary>
/// Base class for the Specification pattern. Encapsulates a business rule
/// as a reusable, composable, testable expression.
/// </summary>
/// <typeparam name="T">The type this specification applies to.</typeparam>
public abstract class Specification<T> where T : class
{
    /// <summary>
    /// Returns the expression that defines this specification's criteria.
    /// EF Core translates this to SQL when used in queries.
    /// </summary>
    public abstract Expression<Func<T, bool>> ToExpression();

    /// <summary>
    /// Evaluates the specification against an entity in memory.
    /// Use for unit testing and domain validation, not for database queries.
    /// </summary>
    public bool IsSatisfiedBy(T entity) => ToExpression().Compile()(entity);

    public bool IsNotSatisfiedBy(T entity) => !IsSatisfiedBy(entity);

    /// <summary>
    /// Combines this specification with another using AND.
    /// </summary>
    public Specification<T> And(Specification<T> other) => new AndSpecification<T>(this, other);

    /// <summary>
    /// Combines this specification with another using OR.
    /// </summary>
    public Specification<T> Or(Specification<T> other) => new OrSpecification<T>(this, other);

    /// <summary>
    /// Negates this specification.
    /// </summary>
    public Specification<T> Not => new NotSpecification<T>(this);

    /// <summary>
    /// Starts a fluent AND chain. Use with entity-specific extension methods.
    /// </summary>
    public SpecChain<T> And() => new(this, false);

    /// <summary>
    /// Starts a fluent OR chain. Use with entity-specific extension methods.
    /// </summary>
    public SpecChain<T> Or() => new(this, true);
}
