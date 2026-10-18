using System;
using System.Collections.Generic;
using System.Linq;

namespace PricingService.Domain;

public sealed class Tariff
{
    public Tariff(string code)
    {
        Code = string.IsNullOrWhiteSpace(code) ? throw new ArgumentException("Tariff code is required.", nameof(code)) : code;
    }

    public string Code { get; }
    public BasePremiumRuleSet BasePremiumRules { get; } = new();
    public DiscountMarkupRuleSet DiscountMarkupRules { get; } = new();

    public TariffPrice CalculatePrice(Calculation calculation)
    {
        if (!string.Equals(Code, calculation.ProductCode, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException($"Calculation product '{calculation.ProductCode}' does not match tariff '{Code}'.");
        }

        var prices = new Dictionary<string, CoverPrice>(StringComparer.OrdinalIgnoreCase);
        foreach (var coverCode in calculation.SelectedCovers)
        {
            var rule = BasePremiumRules.Find(coverCode, calculation.Answers)
                       ?? throw new InvalidOperationException($"No pricing rule exists for cover '{coverCode}'.");
            prices[coverCode] = new CoverPrice(coverCode, rule.Calculate(calculation.Answers));
        }

        var multiplier = DiscountMarkupRules
            .Where(rule => rule.Matches(calculation.Answers))
            .Aggregate(1M, (current, rule) => current * rule.Multiplier);

        foreach (var cover in prices.Values)
        {
            cover.ApplyMultiplier(multiplier);
        }

        return new TariffPrice(prices, prices.Values.Sum(x => x.Price));
    }
}

public sealed class TariffPrice
{
    public TariffPrice(IReadOnlyDictionary<string, CoverPrice> covers, decimal totalPremium)
    {
        Covers = covers;
        TotalPremium = totalPremium;
    }

    public IReadOnlyDictionary<string, CoverPrice> Covers { get; }
    public decimal TotalPremium { get; }
}

public sealed class CoverPrice
{
    internal CoverPrice(string code, decimal price)
    {
        Code = code;
        Price = price;
    }

    public string Code { get; }
    public decimal Price { get; private set; }

    internal void ApplyMultiplier(decimal multiplier) => Price *= multiplier;
}

public sealed class BasePremiumRuleSet
{
    private readonly List<BasePriceRule> rules = new();

    public void AddBasePriceRule(string coverCode, string? condition, string expression)
    {
        rules.Add(new BasePriceRule(coverCode, condition, expression));
    }

    internal BasePriceRule? Find(string coverCode, IReadOnlyDictionary<string, object> answers)
    {
        return rules.FirstOrDefault(rule =>
            string.Equals(rule.CoverCode, coverCode, StringComparison.OrdinalIgnoreCase) && rule.Matches(answers));
    }
}

public sealed class BasePriceRule
{
    internal BasePriceRule(string coverCode, string? condition, string expression)
    {
        CoverCode = coverCode;
        Condition = condition;
        Expression = expression;
    }

    public string CoverCode { get; }
    public string? Condition { get; }
    public string Expression { get; }

    internal bool Matches(IReadOnlyDictionary<string, object> answers)
    {
        return string.IsNullOrWhiteSpace(Condition) || RuleExpressionEvaluator.ToBoolean(RuleExpressionEvaluator.Evaluate(Condition, answers));
    }

    internal decimal Calculate(IReadOnlyDictionary<string, object> answers)
    {
        return RuleExpressionEvaluator.ToDecimal(RuleExpressionEvaluator.Evaluate(Expression, answers));
    }
}

public sealed class DiscountMarkupRuleSet
{
    private readonly List<DiscountMarkupRule> rules = new();

    public void AddPercentMarkup(string condition, decimal multiplier)
    {
        rules.Add(new DiscountMarkupRule(condition, multiplier));
    }

    internal IEnumerable<DiscountMarkupRule> Where(Func<DiscountMarkupRule, bool> predicate) => rules.Where(predicate);
}

public sealed class DiscountMarkupRule
{
    internal DiscountMarkupRule(string condition, decimal multiplier)
    {
        Condition = condition;
        Multiplier = multiplier;
    }

    public string Condition { get; }
    public decimal Multiplier { get; }

    internal bool Matches(IReadOnlyDictionary<string, object> answers)
    {
        return RuleExpressionEvaluator.ToBoolean(RuleExpressionEvaluator.Evaluate(Condition, answers));
    }
}
