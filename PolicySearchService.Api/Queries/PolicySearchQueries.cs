namespace PolicySearchService.Api.Queries;

public class FindPolicyResult
{
    public List<Dtos.PolicyDto> Policies { get; set; } = new();
}
