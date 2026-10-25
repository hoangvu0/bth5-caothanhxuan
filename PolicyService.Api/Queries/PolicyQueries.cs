namespace PolicyService.Api.Queries;

public class GetPolicyDetailsQueryResult
{
    public PolicyDetailsDto Policy { get; set; } = new();
}

public class PolicyDetailsDto
{
    public string Number { get; set; } = string.Empty;
    public DateTime DateFrom { get; set; }
    public DateTime DateTo { get; set; }
    public string PolicyHolder { get; set; } = string.Empty;
    public decimal TotalPremium { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public IList<string> Covers { get; set; } = new List<string>();
}
