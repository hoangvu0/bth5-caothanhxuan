using System;
using System.Collections.Generic;
using System.Globalization;

namespace PricingService.Domain;

internal static class RuleExpressionEvaluator
{
    public static object? Evaluate(string expression, IReadOnlyDictionary<string, object> variables)
    {
        if (string.IsNullOrWhiteSpace(expression))
        {
            throw new FormatException("A rule expression cannot be empty.");
        }

        return new Parser(expression, variables).Parse();
    }

    public static bool ToBoolean(object? value)
    {
        return value switch
        {
            bool boolean => boolean,
            decimal number => number != 0M,
            null => false,
            _ => bool.TryParse(Convert.ToString(value, CultureInfo.InvariantCulture), out var parsed) && parsed
        };
    }

    public static decimal ToDecimal(object? value)
    {
        if (value is decimal decimalValue) return decimalValue;
        if (value is IConvertible convertible)
        {
            return convertible.ToDecimal(CultureInfo.InvariantCulture);
        }

        throw new InvalidOperationException($"Rule result '{value}' is not numeric.");
    }

    private sealed class Parser
    {
        private readonly string text;
        private readonly IReadOnlyDictionary<string, object> variables;
        private int position;

        public Parser(string text, IReadOnlyDictionary<string, object> variables)
        {
            this.text = text;
            this.variables = variables;
        }

        public object? Parse()
        {
            var value = ParseConditional();
            SkipWhitespace();
            if (position != text.Length) throw Error($"Unexpected token near '{text[position..]}'.");
            return value;
        }

        private object? ParseConditional()
        {
            var condition = ParseOr();
            SkipWhitespace();
            if (!Match('?')) return condition;
            var whenTrue = ParseConditional();
            Expect(':');
            var whenFalse = ParseConditional();
            return ToBoolean(condition) ? whenTrue : whenFalse;
        }

        private object? ParseOr()
        {
            var value = ParseAnd();
            while (Match("||")) value = ToBoolean(value) || ToBoolean(ParseAnd());
            return value;
        }

        private object? ParseAnd()
        {
            var value = ParseEquality();
            while (Match("&&")) value = ToBoolean(value) && ToBoolean(ParseEquality());
            return value;
        }

        private object? ParseEquality()
        {
            var value = ParseComparison();
            while (true)
            {
                if (Match("==")) value = AreEqual(value, ParseComparison());
                else if (Match("!=")) value = !AreEqual(value, ParseComparison());
                else return value;
            }
        }

        private object? ParseComparison()
        {
            var value = ParseAdditive();
            while (true)
            {
                if (Match(">=")) value = Compare(value, ParseAdditive()) >= 0;
                else if (Match("<=")) value = Compare(value, ParseAdditive()) <= 0;
                else if (Match('>')) value = Compare(value, ParseAdditive()) > 0;
                else if (Match('<')) value = Compare(value, ParseAdditive()) < 0;
                else return value;
            }
        }

        private object? ParseAdditive()
        {
            var value = ParseMultiplicative();
            while (true)
            {
                if (Match('+')) value = Add(value, ParseMultiplicative());
                else if (Match('-')) value = ToDecimal(value) - ToDecimal(ParseMultiplicative());
                else return value;
            }
        }

        private object? ParseMultiplicative()
        {
            var value = ParseUnary();
            while (true)
            {
                if (Match('*')) value = ToDecimal(value) * ToDecimal(ParseUnary());
                else if (Match('/')) value = ToDecimal(value) / ToDecimal(ParseUnary());
                else return value;
            }
        }

        private object? ParseUnary()
        {
            if (Match('!')) return !ToBoolean(ParseUnary());
            if (Match('-')) return -ToDecimal(ParseUnary());
            return ParsePrimary();
        }

        private object? ParsePrimary()
        {
            SkipWhitespace();
            if (Match('('))
            {
                var parenthesizedValue = ParseConditional();
                Expect(')');
                return parenthesizedValue;
            }

            if (position < text.Length && text[position] == '"') return ParseString();
            if (position < text.Length && (char.IsDigit(text[position]) || text[position] == '.')) return ParseNumber();

            var identifier = ParseIdentifier();
            if (identifier is null) throw Error("Expected a value.");
            if (identifier.Equals("true", StringComparison.OrdinalIgnoreCase)) return true;
            if (identifier.Equals("false", StringComparison.OrdinalIgnoreCase)) return false;
            if (variables.TryGetValue(identifier, out var variableValue)) return variableValue;
            throw Error($"Unknown variable '{identifier}'.");
        }

        private string ParseString()
        {
            position++;
            var value = string.Empty;
            while (position < text.Length)
            {
                var current = text[position++];
                if (current == '"') return value;
                if (current == '\\' && position < text.Length)
                {
                    var escaped = text[position++];
                    value += escaped switch { 'n' => "\n", 'r' => "\r", 't' => "\t", _ => escaped.ToString() };
                }
                else value += current;
            }

            throw Error("Unterminated string literal.");
        }

        private decimal ParseNumber()
        {
            SkipWhitespace();
            var start = position;
            while (position < text.Length && (char.IsDigit(text[position]) || text[position] == '.')) position++;
            if (position < text.Length && (text[position] == 'M' || text[position] == 'm')) position++;
            var token = text[start..position].TrimEnd('M', 'm');
            if (decimal.TryParse(token, NumberStyles.Number, CultureInfo.InvariantCulture, out var number)) return number;
            throw Error($"Invalid number '{token}'.");
        }

        private string? ParseIdentifier()
        {
            SkipWhitespace();
            if (position >= text.Length || !(char.IsLetter(text[position]) || text[position] == '_')) return null;
            var start = position++;
            while (position < text.Length && (char.IsLetterOrDigit(text[position]) || text[position] == '_')) position++;
            return text[start..position];
        }

        private void Expect(char character)
        {
            if (!Match(character)) throw Error($"Expected '{character}'.");
        }

        private bool Match(char character)
        {
            SkipWhitespace();
            if (position < text.Length && text[position] == character) { position++; return true; }
            return false;
        }

        private bool Match(string value)
        {
            SkipWhitespace();
            if (text.AsSpan(position).StartsWith(value, StringComparison.Ordinal)) { position += value.Length; return true; }
            return false;
        }

        private void SkipWhitespace() { while (position < text.Length && char.IsWhiteSpace(text[position])) position++; }
        private FormatException Error(string message) => new($"{message} Expression: '{text}'.");

        private static object Add(object? left, object? right)
        {
            if (left is string || right is string) return $"{left}{right}";
            return ToDecimal(left) + ToDecimal(right);
        }

        private static bool AreEqual(object? left, object? right)
        {
            if (left is null || right is null) return left is null && right is null;
            if (left is IConvertible && right is IConvertible && left is not bool && right is not bool)
            {
                try { return ToDecimal(left) == ToDecimal(right); } catch (FormatException) { }
            }
            return string.Equals(Convert.ToString(left, CultureInfo.InvariantCulture), Convert.ToString(right, CultureInfo.InvariantCulture), StringComparison.OrdinalIgnoreCase);
        }

        private static int Compare(object? left, object? right)
        {
            if (left is string leftString && right is string rightString) return string.Compare(leftString, rightString, StringComparison.OrdinalIgnoreCase);
            return ToDecimal(left).CompareTo(ToDecimal(right));
        }
    }
}
