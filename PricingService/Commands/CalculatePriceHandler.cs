using FluentValidation;
using MediatR;
using PricingService.Api.Commands;
using PricingService.DataAccess;
using PricingService.Domain;

namespace PricingService.Commands;

public sealed class CalculatePriceHandler : IRequestHandler<CalculatePriceCommand, CalculatePriceResult>
{
    private readonly IDataStore dataStore;

    public CalculatePriceHandler(IDataStore dataStore)
    {
        this.dataStore = dataStore;
    }

    public async Task<CalculatePriceResult> Handle(CalculatePriceCommand request, CancellationToken cancellationToken)
    {
        var validation = new CalculatePriceCommandValidator().Validate(request);
        if (!validation.IsValid) throw new ValidationException(validation.Errors);

        Tariff tariff;
        try
        {
            tariff = await dataStore.Tariffs[request.ProductCode];
        }
        catch (KeyNotFoundException)
        {
            throw new KeyNotFoundException($"Unknown product code '{request.ProductCode}'.");
        }
        var answers = request.Answers.ToDictionary(answer => answer.QuestionCode, answer => answer.GetAnswer());
        var calculation = new Calculation(request.ProductCode, request.PolicyFrom, request.PolicyTo,
            request.SelectedCovers, answers);
        var result = tariff.CalculatePrice(calculation);

        return new CalculatePriceResult
        {
            TotalPrice = result.TotalPremium,
            CoverPrices = result.Covers.ToDictionary(pair => pair.Key, pair => pair.Value.Price)
        };
    }
}
