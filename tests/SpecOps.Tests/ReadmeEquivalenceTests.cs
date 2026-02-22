using FluentAssertions;
using static SpecOps.Tests.Specs;

namespace SpecOps.Tests;

public class ReadmeEquivalenceTests
{
    private static readonly Func<Transaction, bool> RawWhereClause = t =>
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
            t.ReviewedBy != null);

    private static readonly Specification<Transaction> SpecOpsQuery =
        ActiveAccount()
            .And().LargeTransaction(10_000m)
            .And().InCurrency("GBP")
            .And().NotReversed()
            .And().HighRisk()
            .And().SettledWithin(30)
            .And().ReviewComplete();

    public static TheoryData<string, Transaction> AllTransactions => new()
    {
        {
            "Flagged: high risk score, active, large, GBP, recent, reviewed",
            new Transaction
            {
                Account = new Account { IsActive = true, IsFrozen = false },
                Amount = 25_000m,
                Currency = "GBP",
                Status = TransactionStatus.Settled,
                RiskScore = 0.9m,
                CounterpartyCountry = "GB",
                SettlementDate = DateTime.UtcNow.AddDays(-5),
                RequiresManualReview = false,
                ReviewedBy = null
            }
        },
        {
            "Flagged: foreign large transfer (high risk via country+amount)",
            new Transaction
            {
                Account = new Account { IsActive = true, IsFrozen = false },
                Amount = 75_000m,
                Currency = "GBP",
                Status = TransactionStatus.Settled,
                RiskScore = 0.2m,
                CounterpartyCountry = "US",
                SettlementDate = DateTime.UtcNow.AddDays(-1),
                RequiresManualReview = true,
                ReviewedBy = "Alice"
            }
        },
        {
            "Excluded: account is frozen",
            new Transaction
            {
                Account = new Account { IsActive = true, IsFrozen = true },
                Amount = 25_000m,
                Currency = "GBP",
                Status = TransactionStatus.Settled,
                RiskScore = 0.9m,
                CounterpartyCountry = "GB",
                SettlementDate = DateTime.UtcNow.AddDays(-5),
                RequiresManualReview = false
            }
        },
        {
            "Excluded: account is inactive",
            new Transaction
            {
                Account = new Account { IsActive = false, IsFrozen = false },
                Amount = 25_000m,
                Currency = "GBP",
                Status = TransactionStatus.Settled,
                RiskScore = 0.9m,
                CounterpartyCountry = "GB",
                SettlementDate = DateTime.UtcNow.AddDays(-5),
                RequiresManualReview = false
            }
        },
        {
            "Excluded: amount too small",
            new Transaction
            {
                Account = new Account { IsActive = true, IsFrozen = false },
                Amount = 5_000m,
                Currency = "GBP",
                Status = TransactionStatus.Settled,
                RiskScore = 0.9m,
                CounterpartyCountry = "GB",
                SettlementDate = DateTime.UtcNow.AddDays(-5),
                RequiresManualReview = false
            }
        },
        {
            "Excluded: wrong currency",
            new Transaction
            {
                Account = new Account { IsActive = true, IsFrozen = false },
                Amount = 25_000m,
                Currency = "USD",
                Status = TransactionStatus.Settled,
                RiskScore = 0.9m,
                CounterpartyCountry = "GB",
                SettlementDate = DateTime.UtcNow.AddDays(-5),
                RequiresManualReview = false
            }
        },
        {
            "Excluded: reversed transaction",
            new Transaction
            {
                Account = new Account { IsActive = true, IsFrozen = false },
                Amount = 25_000m,
                Currency = "GBP",
                Status = TransactionStatus.Reversed,
                RiskScore = 0.9m,
                CounterpartyCountry = "GB",
                SettlementDate = DateTime.UtcNow.AddDays(-5),
                RequiresManualReview = false
            }
        },
        {
            "Excluded: low risk, domestic, under 50k",
            new Transaction
            {
                Account = new Account { IsActive = true, IsFrozen = false },
                Amount = 25_000m,
                Currency = "GBP",
                Status = TransactionStatus.Settled,
                RiskScore = 0.3m,
                CounterpartyCountry = "GB",
                SettlementDate = DateTime.UtcNow.AddDays(-5),
                RequiresManualReview = false
            }
        },
        {
            "Excluded: settled too long ago",
            new Transaction
            {
                Account = new Account { IsActive = true, IsFrozen = false },
                Amount = 25_000m,
                Currency = "GBP",
                Status = TransactionStatus.Settled,
                RiskScore = 0.9m,
                CounterpartyCountry = "GB",
                SettlementDate = DateTime.UtcNow.AddDays(-60),
                RequiresManualReview = false
            }
        },
        {
            "Excluded: requires review but not reviewed",
            new Transaction
            {
                Account = new Account { IsActive = true, IsFrozen = false },
                Amount = 25_000m,
                Currency = "GBP",
                Status = TransactionStatus.Settled,
                RiskScore = 0.9m,
                CounterpartyCountry = "GB",
                SettlementDate = DateTime.UtcNow.AddDays(-5),
                RequiresManualReview = true,
                ReviewedBy = null
            }
        },
    };

    [Theory]
    [MemberData(nameof(AllTransactions))]
    public void SpecOps_matches_raw_Where_clause(string scenario, Transaction transaction)
    {
        var rawResult = RawWhereClause(transaction);
        var specResult = SpecOpsQuery.IsSatisfiedBy(transaction);

        specResult.Should().Be(rawResult, because: scenario);
    }

    [Fact]
    public void SpecOps_filters_collection_identically_to_raw_Where()
    {
        var transactions = new List<Transaction>();
        foreach (var entry in AllTransactions)
            transactions.Add((Transaction)entry[1]);

        var rawResults = transactions.Where(RawWhereClause).ToList();
        var specResults = transactions.AsQueryable().WithSpecification(SpecOpsQuery).ToList();

        specResults.Should().BeEquivalentTo(rawResults);
    }
}
