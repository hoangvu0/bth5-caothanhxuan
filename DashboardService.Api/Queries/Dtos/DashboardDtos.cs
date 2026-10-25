using DashboardService.Api.Queries;

namespace DashboardService.Api.Queries.Dtos;

public enum TimeUnit
{
    Day,
    Month,
    Year
}

public class PeriodSalesDto
{
    public string Period { get; set; } = string.Empty;
    public SalesTotalDto Sales { get; set; } = new();
}
