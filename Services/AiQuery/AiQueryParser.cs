using System.Globalization;
using System.Text.RegularExpressions;
using elyra.Models;

namespace elyra.Services.AiQuery;

public static partial class AiQueryParser
{
    [GeneratedRegex(@"(?:>|over|above)\s*£?\s*(\d+(?:\.\d+)?)\s*([kK]?)", RegexOptions.IgnoreCase)]
    private static partial Regex AmountPattern();

    public static AiQueryInterpretation Parse(string? prompt)
    {
        if (string.IsNullOrWhiteSpace(prompt))
        {
            return new AiQueryInterpretation();
        }

        var cleanPrompt = prompt.Trim();
        var query = Normalize(cleanPrompt);

        if (ContainsAny(query, "merchant", "merchants") && ContainsAny(query, "top", "risky", "riskiest", "worst", "highest risk"))
        {
            return new AiQueryInterpretation
            {
                Prompt = cleanPrompt,
                Intent = AiQueryIntent.TopRiskyMerchants
            };
        }

        if (ContainsAny(query, "why", "what", "explain") &&
            ContainsAny(query, "fail", "failure", "failed") &&
            ContainsAny(query, "increase", "increased", "up", "spike", "spiked", "rising"))
        {
            return new AiQueryInterpretation
            {
                Prompt = cleanPrompt,
                Intent = AiQueryIntent.WhyFailuresIncreased
            };
        }

        if (ContainsAny(query, "failed", "failures", "failed payments", "failed transactions") &&
            (query.Contains(">") || query.Contains("over") || query.Contains("above")))
        {
            var threshold = TryParseAmountThreshold(query);
            if (threshold is not null)
            {
                return new AiQueryInterpretation
                {
                    Prompt = cleanPrompt,
                    Intent = AiQueryIntent.FailedAboveAmount,
                    AmountThreshold = threshold
                };
            }
        }

        if (ContainsAny(query, "worst", "riskiest", "highest risk", "most dangerous", "highest-risk") &&
            ContainsAny(query, "transaction", "transactions", "payment", "payments"))
        {
            var timeWindow = TryParseTimeWindow(query);

            return new AiQueryInterpretation
            {
                Prompt = cleanPrompt,
                Intent = AiQueryIntent.WorstTransactions,
                LookbackWindow = timeWindow.Window,
                TimeWindowLabel = timeWindow.Label
            };
        }

        return new AiQueryInterpretation
        {
            Prompt = cleanPrompt,
            Intent = AiQueryIntent.Unknown
        };
    }

    private static string Normalize(string prompt) =>
        Regex.Replace(prompt.ToLowerInvariant(), @"\s+", " ").Trim();

    private static decimal? TryParseAmountThreshold(string query)
    {
        var match = AmountPattern().Match(query);
        if (!match.Success)
        {
            return null;
        }

        var rawValue = match.Groups[1].Value;
        var suffix = match.Groups[2].Value;
        if (!decimal.TryParse(rawValue, NumberStyles.Number, CultureInfo.InvariantCulture, out var amount))
        {
            return null;
        }

        if (string.Equals(suffix, "k", StringComparison.OrdinalIgnoreCase))
        {
            amount *= 1000m;
        }

        return amount;
    }

    private static bool ContainsAny(string query, params string[] phrases) =>
        phrases.Any(query.Contains);

    private static (TimeSpan? Window, string Label) TryParseTimeWindow(string query)
    {
        if (query.Contains("last month") || query.Contains("past month"))
        {
            return (TimeSpan.FromDays(30), "the last month");
        }

        if (query.Contains("last 30 days") || query.Contains("past 30 days"))
        {
            return (TimeSpan.FromDays(30), "the last 30 days");
        }

        if (query.Contains("last week") || query.Contains("past week"))
        {
            return (TimeSpan.FromDays(7), "the last week");
        }

        if (query.Contains("today"))
        {
            return (TimeSpan.FromDays(1), "today");
        }

        return (null, "the current dataset");
    }
}
