namespace PricingService.Domain;

public sealed class Tariff
{
    public Tariff(string code)
    {
        Code = code;
    }

    public string Code { get; }
    public BasePremiumRules BasePremiumRules { get; } = new();
    public DiscountMarkupRules DiscountMarkupRules { get; } = new();

    public CalculationResult CalculatePrice(Calculation calculation)
    {
        var covers = new Dictionary<string, CoverPrice>();

        foreach (var cover in calculation.SelectedCovers)
        {
            var rule = BasePremiumRules.Rules.FirstOrDefault(candidate =>
                candidate.CoverCode == cover &&
                (candidate.Condition is null || RuleExpressionEvaluator.EvaluateBoolean(candidate.Condition, calculation.Answers)));

            if (rule is null)
                throw new InvalidOperationException($"No pricing rule found for cover '{cover}' on tariff '{Code}'.");

            covers[cover] = new CoverPrice { Price = RuleExpressionEvaluator.EvaluateDecimal(rule.Expression, calculation.Answers) };
        }

        foreach (var markup in DiscountMarkupRules.Rules)
        {
            if (markup.Condition is null || RuleExpressionEvaluator.EvaluateBoolean(markup.Condition, calculation.Answers))
            {
                foreach (var cover in covers.Keys.ToList())
                    covers[cover] = new CoverPrice { Price = covers[cover].Price * markup.Factor };
            }
        }

        return new CalculationResult
        {
            Covers = covers,
            TotalPremium = covers.Values.Sum(cover => cover.Price)
        };
    }
}

public sealed class BasePremiumRules
{
    internal List<BasePremiumRule> Rules { get; } = new();

    public void AddBasePriceRule(string coverCode, string? condition, string expression)
    {
        Rules.Add(new BasePremiumRule(coverCode, condition, expression));
    }
}

public sealed record BasePremiumRule(string CoverCode, string? Condition, string Expression);

public sealed class DiscountMarkupRules
{
    internal List<DiscountMarkupRule> Rules { get; } = new();

    public void AddPercentMarkup(string? condition, decimal factor)
    {
        Rules.Add(new DiscountMarkupRule(condition, factor));
    }
}

public sealed record DiscountMarkupRule(string? Condition, decimal Factor);
