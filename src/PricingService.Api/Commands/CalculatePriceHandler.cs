using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using PricingService.Api.Commands;
using PricingService.Api.Commands.Dto;
using PricingService.Domain;

namespace PricingService.Commands;

public interface IDataStore
{
    IDictionary<string, Task<Tariff>> Tariffs { get; }
}

public sealed class CalculatePriceHandler : IRequestHandler<CalculatePriceCommand, CalculatePriceResult>
{
    private readonly IDataStore dataStore;
    private readonly IValidator<CalculatePriceCommand> validator = new CalculatePriceCommandValidator();

    public CalculatePriceHandler(IDataStore dataStore)
    {
        this.dataStore = dataStore;
    }

    public async Task<CalculatePriceResult> Handle(CalculatePriceCommand request, CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        var answers = request.Answers.ToDictionary(answer => answer.QuestionCode, answer => answer.GetAnswer(),
            System.StringComparer.OrdinalIgnoreCase);
        var tariff = await dataStore.Tariffs[request.ProductCode];
        if (tariff is null) return CalculatePriceResult.Empty();

        var calculation = new Calculation(
            request.ProductCode,
            request.PolicyFrom,
            request.PolicyTo,
            request.SelectedCovers,
            answers);
        var price = tariff.CalculatePrice(calculation);

        return new CalculatePriceResult
        {
            TotalPrice = price.TotalPremium,
            CoverPrices = price.Covers.ToDictionary(x => x.Key, x => x.Value.Price)
        };
    }
}
