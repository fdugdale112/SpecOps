using System.Linq.Expressions;

namespace SpecOps.Tests;

public enum TransactionStatus { Pending, Settled, Reversed }

public class Account
{
    public bool IsActive { get; init; }
    public bool IsFrozen { get; init; }
}

public class Transaction
{
    public Account Account { get; init; } = new();
    public decimal Amount { get; init; }
    public string Currency { get; init; } = "";
    public TransactionStatus Status { get; init; }
    public decimal RiskScore { get; init; }
    public string CounterpartyCountry { get; init; } = "";
    public DateTime SettlementDate { get; init; }
    public bool RequiresManualReview { get; init; }
    public string? ReviewedBy { get; init; }
}

public class ActiveAccount : Specification<Transaction>
{
    public override Expression<Func<Transaction, bool>> ToExpression()
        => t => t.Account.IsActive && !t.Account.IsFrozen;
}

public class LargeTransaction(decimal threshold) : Specification<Transaction>
{
    public override Expression<Func<Transaction, bool>> ToExpression()
        => t => t.Amount > threshold;
}

public class InCurrency(string currency) : Specification<Transaction>
{
    public override Expression<Func<Transaction, bool>> ToExpression()
        => t => t.Currency == currency;
}

public class NotReversed : Specification<Transaction>
{
    public override Expression<Func<Transaction, bool>> ToExpression()
        => t => t.Status != TransactionStatus.Reversed;
}

public class HighRisk : Specification<Transaction>
{
    public override Expression<Func<Transaction, bool>> ToExpression()
        => t => t.RiskScore > 0.7m ||
                (t.CounterpartyCountry != "GB" && t.Amount > 50_000m);
}

public class SettledWithin(int days) : Specification<Transaction>
{
    public override Expression<Func<Transaction, bool>> ToExpression()
        => t => t.SettlementDate >= DateTime.UtcNow.AddDays(-days);
}

public class ReviewComplete : Specification<Transaction>
{
    public override Expression<Func<Transaction, bool>> ToExpression()
        => t => !t.RequiresManualReview || t.ReviewedBy != null;
}
