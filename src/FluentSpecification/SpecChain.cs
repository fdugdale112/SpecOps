namespace FluentSpecification;

/// <summary>
/// Intermediate type for fluent specification chaining.
/// Carries the left-hand specification and the combining mode (AND/OR).
/// </summary>
public record SpecChain<T>(Specification<T> Left, bool IsOr) where T : class
{
    public Specification<T> Combine(Specification<T> right)
        => IsOr ? Left.Or(right) : Left.And(right);
}
