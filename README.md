# SpecOps

A fluent, composable Specification pattern for .NET. Build readable, testable business rules as expressions that translate directly to SQL via EF Core.

## Installation

```bash
dotnet add package SpecOps
```

## Quick Start

### 1. Define a specification

```csharp
using System.Linq.Expressions;
using SpecOps;

public class ActiveClient : Specification<Client>
{
    public override Expression<Func<Client, bool>> ToExpression()
        => client => client.IsActive;
}
```

### 2. Add factory methods for readability

```csharp
public static partial class ClientSpec
{
    public static Specification<Client> Active() => new ActiveClient();
    public static Specification<Client> Active(this SpecChain<Client> chain)
        => chain.Combine(new ActiveClient());
}
```

### 3. Use in queries

```csharp
using static ClientSpec;

// Simple
var activeClients = await dbContext.Clients
    .WithSpecification(Active())
    .ToListAsync();

// Composed with And/Or
var spec = Active()
    .And().NameContaining("Acme")
    .Or().CreatedAfter(lastMonth);

var results = await dbContext.Clients
    .WithSpecification(spec)
    .ToListAsync();
```

### 4. Test in isolation

```csharp
var client = new Client { Name = "Acme", IsActive = true };

Active().IsSatisfiedBy(client).Should().BeTrue();
Active().IsNotSatisfiedBy(client).Should().BeFalse();
```

## Composition

### Explicit grouping (parenthesised)

```csharp
// email AND (name OR name)
var spec = ByEmail("fred@acme.com")
    .And(NameContaining("Acme").Or(NameContaining("Widget")));
```

### Fluent chaining (left-to-right)

```csharp
// (email AND name) OR name — evaluated left-to-right
var spec = ByEmail("fred@acme.com")
    .And().NameContaining("Acme")
    .Or().NameContaining("Widget");
```

### Negation

```csharp
var notActive = Active().Not;
```

## EF Core Integration

Specifications produce `Expression<Func<T, bool>>`, which EF Core translates to SQL:

```csharp
dbContext.Clients
    .WithSpecification(Active().And().NameContaining("Acme"))
    .ToListAsync();

// Generates: SELECT ... WHERE IsActive = true AND Name LIKE '%Acme%'
```

## License

MIT
