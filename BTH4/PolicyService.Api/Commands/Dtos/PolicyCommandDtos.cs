namespace PolicyService.Api.Commands.Dtos;

public class PersonDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string TaxId { get; set; } = string.Empty;
}

public class AddressDto
{
    public string Country { get; set; } = string.Empty;
    public string ZipCode { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Street { get; set; } = string.Empty;
}

public enum QuestionType
{
    Numeric,
    Choice
}

public abstract class QuestionAnswer
{
    public string QuestionCode { get; set; } = string.Empty;
    public abstract QuestionType QuestionType { get; }
}

public class ChoiceQuestionAnswer : QuestionAnswer
{
    public override QuestionType QuestionType => QuestionType.Choice;
    public string Answer { get; set; } = string.Empty;
}

public class NumericQuestionAnswer : QuestionAnswer
{
    public override QuestionType QuestionType => QuestionType.Numeric;
    public decimal Answer { get; set; }
}
