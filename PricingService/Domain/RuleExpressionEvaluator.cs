namespace PricingService.Domain;

internal static class RuleExpressionEvaluator
{
    public static decimal EvaluateDecimal(string expression, IDictionary<string, object> variables)
    {
        var value = new Parser(expression, variables).Parse();
        return Convert.ToDecimal(value, System.Globalization.CultureInfo.InvariantCulture);
    }

    public static bool EvaluateBoolean(string expression, IDictionary<string, object> variables)
    {
        var value = new Parser(expression, variables).Parse();
        return value is bool boolean ? boolean : throw new InvalidOperationException($"Expression '{expression}' is not boolean.");
    }

    private sealed class Parser
    {
        private readonly string expression;
        private readonly IDictionary<string, object> variables;
        private int position;

        public Parser(string expression, IDictionary<string, object> variables)
        {
            this.expression = expression;
            this.variables = variables;
        }

        public object Parse()
        {
            var result = ParseConditional();
            SkipWhitespace();
            if (position != expression.Length)
                throw new FormatException($"Unexpected token at position {position} in '{expression}'.");
            return result;
        }

        private object ParseConditional()
        {
            var condition = ParseComparison();
            SkipWhitespace();
            if (!Match('?')) return condition;
            var whenTrue = ParseConditional();
            Expect(':');
            var whenFalse = ParseConditional();
            return ToBoolean(condition) ? whenTrue : whenFalse;
        }

        private object ParseComparison()
        {
            var left = ParseAdditive();
            while (true)
            {
                SkipWhitespace();
                var op = Match("==") ? "==" : Match("!=") ? "!=" : Match(">=") ? ">=" :
                    Match("<=") ? "<=" : Match('>') ? ">" : Match('<') ? "<" : null;
                if (op is null) return left;
                var right = ParseAdditive();
                left = op switch
                {
                    "==" => EqualsValue(left, right),
                    "!=" => !EqualsValue(left, right),
                    ">" => ToDecimal(left) > ToDecimal(right),
                    "<" => ToDecimal(left) < ToDecimal(right),
                    ">=" => ToDecimal(left) >= ToDecimal(right),
                    "<=" => ToDecimal(left) <= ToDecimal(right),
                    _ => throw new InvalidOperationException()
                };
            }
        }

        private object ParseAdditive()
        {
            var value = ParseMultiplicative();
            while (true)
            {
                SkipWhitespace();
                if (Match('+')) value = ToDecimal(value) + ToDecimal(ParseMultiplicative());
                else if (Match('-')) value = ToDecimal(value) - ToDecimal(ParseMultiplicative());
                else return value;
            }
        }

        private object ParseMultiplicative()
        {
            var value = ParsePrimary();
            while (true)
            {
                SkipWhitespace();
                if (Match('*')) value = ToDecimal(value) * ToDecimal(ParsePrimary());
                else if (Match('/')) value = ToDecimal(value) / ToDecimal(ParsePrimary());
                else return value;
            }
        }

        private object ParsePrimary()
        {
            SkipWhitespace();
            if (Match('('))
            {
                var grouped = ParseConditional();
                Expect(')');
                return grouped;
            }
            if (position < expression.Length && expression[position] == '"') return ParseString();
            if (position < expression.Length && (char.IsDigit(expression[position]) || expression[position] == '.'))
                return ParseNumber();

            var identifier = ParseIdentifier();
            if (identifier.Length == 0)
                throw new FormatException($"Expected a value at position {position} in '{expression}'.");
            return variables.TryGetValue(identifier, out var value)
                ? value
                : throw new KeyNotFoundException($"No answer was provided for '{identifier}'.");
        }

        private string ParseString()
        {
            position++;
            var start = position;
            while (position < expression.Length && expression[position] != '"') position++;
            if (position >= expression.Length) throw new FormatException("Unterminated string literal.");
            var value = expression[start..position];
            position++;
            return value;
        }

        private decimal ParseNumber()
        {
            var start = position;
            while (position < expression.Length && (char.IsDigit(expression[position]) || expression[position] == '.')) position++;
            if (position < expression.Length && (expression[position] == 'M' || expression[position] == 'm')) position++;
            return decimal.Parse(expression[start..position].TrimEnd('M', 'm'), System.Globalization.CultureInfo.InvariantCulture);
        }

        private string ParseIdentifier()
        {
            var start = position;
            while (position < expression.Length && (char.IsLetterOrDigit(expression[position]) || expression[position] == '_')) position++;
            return expression[start..position];
        }

        private bool Match(char value)
        {
            if (position < expression.Length && expression[position] == value) { position++; return true; }
            return false;
        }

        private bool Match(string value)
        {
            if (expression.AsSpan(position).StartsWith(value, StringComparison.Ordinal)) { position += value.Length; return true; }
            return false;
        }

        private void Expect(char value)
        {
            SkipWhitespace();
            if (!Match(value)) throw new FormatException($"Expected '{value}' in '{expression}'.");
        }

        private void SkipWhitespace()
        {
            while (position < expression.Length && char.IsWhiteSpace(expression[position])) position++;
        }

        private static decimal ToDecimal(object value) => Convert.ToDecimal(value, System.Globalization.CultureInfo.InvariantCulture);
        private static bool ToBoolean(object value) => value is bool boolean && boolean;
        private static bool EqualsValue(object left, object right) => left is string || right is string
            ? string.Equals(left?.ToString(), right?.ToString(), StringComparison.Ordinal)
            : ToDecimal(left) == ToDecimal(right);
    }
}
