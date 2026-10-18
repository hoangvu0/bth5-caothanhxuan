using System;
using System.Collections.Generic;

namespace PricingService.Domain;

public sealed class Calculation
{
    public Calculation(
        string productCode,
        DateTimeOffset policyFrom,
        DateTimeOffset policyTo,
        IReadOnlyCollection<string> selectedCovers,
        IReadOnlyDictionary<string, object> answers)
    {
        ProductCode = productCode ?? throw new ArgumentNullException(nameof(productCode));
        PolicyFrom = policyFrom;
        PolicyTo = policyTo;
        SelectedCovers = selectedCovers ?? throw new ArgumentNullException(nameof(selectedCovers));
        Answers = answers ?? throw new ArgumentNullException(nameof(answers));
    }

    public string ProductCode { get; }
    public DateTimeOffset PolicyFrom { get; }
    public DateTimeOffset PolicyTo { get; }
    public IReadOnlyCollection<string> SelectedCovers { get; }
    public IReadOnlyDictionary<string, object> Answers { get; }
}
