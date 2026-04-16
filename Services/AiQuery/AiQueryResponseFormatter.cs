using System.Globalization;
using elyra.Models;

namespace elyra.Services.AiQuery;

public static class AiQueryResponseFormatter
{
    private static readonly string[] FallbackExamples =
    [
        "show failed > £3k",
        "why did failures increase",
        "top risky merchants"
    ];

    public static AiQueryResponse Format(
        AiQueryInterpretation interpretation,
        AiQueryExecutionResult executionResult)
    {
        return interpretation.Intent switch
        {
            AiQueryIntent.FailedAboveAmount => FormatFailedAbove(executionResult, interpretation),
            AiQueryIntent.WhyFailuresIncreased => FormatFailureIncrease(executionResult, interpretation),
            AiQueryIntent.TopRiskyMerchants => FormatTopRisky(executionResult, interpretation),
            _ => FormatFallback(interpretation)
        };
    }

    private static AiQueryResponse FormatFailedAbove(
        AiQueryExecutionResult executionResult,
        AiQueryInterpretation interpretation)
    {
        var threshold = executionResult.AmountThreshold ?? interpretation.AmountThreshold ?? 3000m;
        var summary =
            $"Found {executionResult.MatchedCount} failed transactions above £{threshold.ToString("N0", CultureInfo.InvariantCulture)}. " +
            "The grid is now focused on higher-value failures for fast triage.";

        return new AiQueryResponse
        {
            Prompt = interpretation.Prompt,
            Summary = summary
        };
    }

    private static AiQueryResponse FormatFailureIncrease(
        AiQueryExecutionResult executionResult,
        AiQueryInterpretation interpretation)
    {
        var summary =
            $"In the last 8 hours, {executionResult.RecentFailedCount} failed transactions are visible. " +
            $"The largest concentration is on {executionResult.PrimaryDriver}, suggesting rail-specific retry friction.";

        return new AiQueryResponse
        {
            Prompt = interpretation.Prompt,
            Summary = summary
        };
    }

    private static AiQueryResponse FormatTopRisky(
        AiQueryExecutionResult executionResult,
        AiQueryInterpretation interpretation)
    {
        var merchants = executionResult.TopMerchants.Count == 0
            ? "No high-risk merchants found."
            : string.Join(", ", executionResult.TopMerchants);

        var summary =
            $"Top risky merchants by average risk score are: {merchants}. " +
            "The grid now shows their transactions in descending risk order.";

        return new AiQueryResponse
        {
            Prompt = interpretation.Prompt,
            Summary = summary
        };
    }

    private static AiQueryResponse FormatFallback(AiQueryInterpretation interpretation) =>
        new()
        {
            Prompt = interpretation.Prompt,
            IsFallback = true,
            Summary = "I could not map that request to a supported dashboard action yet. Try one of the examples below and I will apply it directly to the grid.",
            Suggestions = FallbackExamples
        };
}
