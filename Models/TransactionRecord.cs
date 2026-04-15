namespace elyra.Models;

public class TransactionRecord
{
    public string TransactionId { get; init; } = string.Empty;

    public string Merchant { get; init; } = string.Empty;

    public decimal Amount { get; init; }

    public string Currency { get; init; } = "GBP";

    public string Status { get; init; } = string.Empty;

    public string PaymentRail { get; init; } = string.Empty;

    public int RiskScore { get; init; }

    public DateTime Timestamp { get; init; }
}
