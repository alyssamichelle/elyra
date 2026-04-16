namespace elyra.Models;

public sealed class AiQueryInterpretation
{
    public string Prompt { get; init; } = string.Empty;

    public AiQueryIntent Intent { get; init; } = AiQueryIntent.Unknown;

    public decimal? AmountThreshold { get; init; }

    public bool IsKnown => Intent != AiQueryIntent.Unknown;
}
