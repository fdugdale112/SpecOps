using System.Linq.Expressions;

namespace SpecOps.Tests;

public class ByEmail(string email) : Specification<TestEntity>
{
    public override Expression<Func<TestEntity, bool>> ToExpression()
        => e => e.Email.Equals(email, StringComparison.OrdinalIgnoreCase);
}

public class NameContaining(string search) : Specification<TestEntity>
{
    public override Expression<Func<TestEntity, bool>> ToExpression()
        => e => e.Name.Contains(search);
}

public class OlderThan(int age) : Specification<TestEntity>
{
    public override Expression<Func<TestEntity, bool>> ToExpression()
        => e => e.Age > age;
}
