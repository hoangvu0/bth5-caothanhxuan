namespace PricingService.Domain;

public sealed class Calculation
{
    public Calculation(
        string productCode,
        DateTimeOffset policyFrom,
        DateTimeOffset policyTo,
        IEnumerable<string> selectedCovers,
        IDictionary<string, object> answers)
    {
        ProductCode = productCode;
        PolicyFrom = policyFrom;
        PolicyTo = policyTo;
        SelectedCovers = selectedCovers.ToList();
        Answers = answers;
    }

    public string ProductCode { get; }
    public DateTimeOffset PolicyFrom { get; }
    public DateTimeOffset PolicyTo { get; }
    public IReadOnlyList<string> SelectedCovers { get; }
    public IDictionary<string, object> Answers { get; }
}

public sealed class CalculationResult
{
    public decimal TotalPremium { get; init; }
    public IDictionary<string, CoverPrice> Covers { get; init; } = new Dictionary<string, CoverPrice>();
}

public sealed class CoverPrice
{
    public decimal Price { get; init; }
}
