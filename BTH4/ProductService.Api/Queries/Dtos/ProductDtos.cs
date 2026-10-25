namespace ProductService.Api.Queries.Dtos;

public class ProductDto
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Image { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public IList<CoverDto> Covers { get; set; } = new List<CoverDto>();
    public IList<QuestionDto> Questions { get; set; } = new List<QuestionDto>();
}

public class CoverDto
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal SumInsured { get; set; }
}

public abstract class QuestionDto
{
    public string QuestionCode { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
}

public class ChoiceQuestionDto : QuestionDto
{
    public IList<ChoiceDto> Choices { get; set; } = new List<ChoiceDto>();
}

public class NumericQuestionDto : QuestionDto
{
}

public class ChoiceDto
{
    public string Code { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
}
