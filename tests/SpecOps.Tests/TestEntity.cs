namespace SpecOps.Tests;

public class TestEntity(string name, string email, int age)
{
    public string Name { get; } = name;
    public string Email { get; } = email;
    public int Age { get; } = age;
}
