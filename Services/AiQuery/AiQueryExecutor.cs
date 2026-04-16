using elyra.Models;

namespace elyra.Services.AiQuery;

public static class AiQueryExecutor
{
    public static AiQueryExecutionResult Execute(
        AiQueryInterpretation interpretation,
        IReadOnlyList<TransactionRecord> allTransactions,
        DateTime now)
    {
        return interpretation.Intent switch
        {
            AiQueryIntent.FailedAboveAmount => ExecuteFailedAboveAmount(interpretation, allTransactions),
            AiQueryIntent.WhyFailuresIncreased => ExecuteFailureIncrease(allTransactions, now),
            AiQueryIntent.TopRiskyMerchants => ExecuteTopRiskyMerchants(allTransactions),
            AiQueryIntent.WorstTransactions => ExecuteWorstTransactions(interpretation, allTransactions, now),
            _ => new AiQueryExecutionResult { Rows = allTransactions }
        };
    }

    private static AiQueryExecutionResult ExecuteFailedAboveAmount(
        AiQueryInterpretation interpretation,
        IReadOnlyList<TransactionRecord> allTransactions)
    {
        var threshold = interpretation.AmountThreshold ?? 3000m;
        var rows = allTransactions
            .Where(x => x.Status == "Failed" && x.Amount > threshold)
            .OrderByDescending(x => x.Amount)
            .ToList();

        return new AiQueryExecutionResult
        {
            Rows = rows,
            AmountThreshold = threshold
        };
    }

    private static AiQueryExecutionResult ExecuteFailureIncrease(
        IReadOnlyList<TransactionRecord> allTransactions,
        DateTime now)
    {
        var windowStart = now.AddHours(-8);
        var rows = allTransactions
            .Where(x => x.Status == "Failed" && x.Timestamp >= windowStart)
            .OrderByDescending(x => x.RiskScore)
            .ThenByDescending(x => x.Timestamp)
            .ToList();

        var driver = rows
            .GroupBy(x => x.PaymentRail)
            .OrderByDescending(g => g.Count())
            .Select(g => g.Key)
            .FirstOrDefault() ?? "mixed rails";
        var topDeclineReason = rows
            .Where(x => x.DeclineReason != "None")
            .GroupBy(x => x.DeclineReason)
            .OrderByDescending(g => g.Count())
            .Select(g => g.Key)
            .FirstOrDefault() ?? "issuer declines";

        return new AiQueryExecutionResult
        {
            Rows = rows,
            RecentFailedCount = rows.Count,
            PrimaryDriver = driver,
            TopDeclineReason = topDeclineReason
        };
    }

    private static AiQueryExecutionResult ExecuteTopRiskyMerchants(IReadOnlyList<TransactionRecord> allTransactions)
    {
        var topMerchants = allTransactions
            .GroupBy(x => x.Merchant)
            .Select(group => new
            {
                Merchant = group.Key,
                AvgRisk = group.Average(x => x.RiskScore)
            })
            .OrderByDescending(x => x.AvgRisk)
            .Take(3)
            .Select(x => x.Merchant)
            .ToList();

        var rows = allTransactions
            .Where(x => topMerchants.Contains(x.Merchant))
            .OrderByDescending(x => x.RiskScore)
            .ThenByDescending(x => x.Amount)
            .ToList();

        return new AiQueryExecutionResult
        {
            Rows = rows,
            TopMerchants = topMerchants
        };
    }

    private static AiQueryExecutionResult ExecuteWorstTransactions(
        AiQueryInterpretation interpretation,
        IReadOnlyList<TransactionRecord> allTransactions,
        DateTime now)
    {
        var timeWindowLabel = interpretation.TimeWindowLabel;
        IEnumerable<TransactionRecord> scopedTransactions = allTransactions;

        if (interpretation.LookbackWindow is { } lookbackWindow)
        {
            var windowStart = now.Subtract(lookbackWindow);
            scopedTransactions = scopedTransactions.Where(x => x.Timestamp >= windowStart);
        }

        var rows = scopedTransactions
            .OrderByDescending(x => x.RiskScore)
            .ThenByDescending(x => x.Status == "Failed")
            .ThenByDescending(x => x.Amount)
            .ThenByDescending(x => x.Timestamp)
            .Take(25)
            .ToList();

        return new AiQueryExecutionResult
        {
            Rows = rows,
            TimeWindowLabel = string.IsNullOrWhiteSpace(timeWindowLabel) ? "the current dataset" : timeWindowLabel
        };
    }
}
