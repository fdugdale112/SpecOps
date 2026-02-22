# SpecOps

A composable Specification pattern for .NET. Replace tangled `Where` clauses with readable, testable, reusable business rules — all translating directly to SQL via EF Core.

## Why?

Queries like this are hard to read, impossible to unit test, and get copy-pasted everywhere with subtle variations:

```csharp
var transactions = await dbContext.Transactions
    .Where(t =>
        t.Account.IsActive &&
        !t.Account.IsFrozen &&
        t.Amount > 10_000m &&
        t.Currency == "GBP" &&
        t.Status != TransactionStatus.Reversed &&
        (t.RiskScore > 0.7m ||
            (t.CounterpartyCountry != "GB" &&
             t.Amount > 50_000m)) &&
        t.SettlementDate >= DateTime.UtcNow.AddDays(-30) &&
        (!t.RequiresManualReview ||
            t.ReviewedBy != null))
    .OrderByDescending(t => t.Amount)
    .ToListAsync();
```

With SpecOps, each rule is named, testable, and reusable:

```csharp
var flagged = ActiveAccount()
    .And().LargeTransaction(10_000m)
    .And().InCurrency("GBP")
    .And().NotReversed()
    .And().HighRisk()
    .And().SettledWithin(30)
    .And().ReviewComplete();

var transactions = await dbContext.Transactions
    .WithSpecification(flagged)
    .OrderByDescending(t => t.Amount)
    .ToListAsync();
```

The query reads like a sentence. EF Core still translates it to a single SQL `WHERE` clause.

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

### 2. Factory methods are generated automatically

SpecOps includes a source generator that creates a `Specs` class with factory and extension methods for every `Specification<T>` in your project. No boilerplate needed.

### 3. Compose and query

```csharp
using static Specs;

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

### Explicit grouping

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
var inactive = Active().Not;
```

## License

MIT
