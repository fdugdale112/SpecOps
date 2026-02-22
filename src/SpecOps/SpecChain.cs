namespace SpecOps;

/// <summary>
/// Intermediate type for fluent specification chaining.
/// Carries the left-hand specification and the combining mode (AND/OR).
/// </summary>
public record SpecChain<T>(Specification<T> Left, bool IsOr, bool Negate = false) where T : class
{
    public SpecChain<T> Not() => this with { Negate = true };

    public Specification<T> Combine(Specification<T> right)
    {
        var effective = Negate ? right.Not : right;
        return IsOr ? Left.Or(effective) : Left.And(effective);
    }
}
