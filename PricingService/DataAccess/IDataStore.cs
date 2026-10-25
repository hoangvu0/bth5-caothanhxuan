using PricingService.Domain;

namespace PricingService.Domain;

public interface IDataStore
{
    IDictionary<string, Task<Tariff>> Tariffs { get; }
}
