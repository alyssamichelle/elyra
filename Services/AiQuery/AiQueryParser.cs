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
        var query = cleanPrompt.ToLowerInvariant();

        if (query.Contains("top") && query.Contains("risk") && query.Contains("merchant"))
        {
            return new AiQueryInterpretation
            {
                Prompt = cleanPrompt,
                Intent = AiQueryIntent.TopRiskyMerchants
            };
        }

        if (query.Contains("why") && query.Contains("fail") && query.Contains("increase"))
        {
            return new AiQueryInterpretation
            {
                Prompt = cleanPrompt,
                Intent = AiQueryIntent.WhyFailuresIncreased
            };
        }

        if (query.Contains("failed") && (query.Contains(">") || query.Contains("over") || query.Contains("above")))
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

        return new AiQueryInterpretation
        {
            Prompt = cleanPrompt,
            Intent = AiQueryIntent.Unknown
        };
    }

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
}
