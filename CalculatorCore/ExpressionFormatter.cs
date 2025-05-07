using System.Diagnostics;
using System.Globalization;

namespace CalculatorCore
{
    public class ExpressionFormatter
    {
        // ==================== РОЗДІЛ 1: ФОРМАТУВАННЯ ЧИСЕЛ ДЛЯ ВІДОБРАЖЕННЯ ====================

        // 1.1 Основний метод форматування чисел
        public string FormatDisplay(double number)
        {
            // Обробка спеціальних значень
            if (double.IsInfinity(number))
                return number > 0 ? "∞" : "-∞";
            if (double.IsNaN(number))
                return "NaN";

            // Визначення, чи потрібна наукова нотація
            bool useScientific = ShouldUseScientificNotation(number);

            if (useScientific)
            {
                return FormatScientific(number);
            }

            return FormatRegular(number);
        }

        // 1.2 Допоміжні методи для форматування чисел
        private bool ShouldUseScientificNotation(double number)
        {
            return Math.Abs(number) >= 1_000_000_000 ||
                   (Math.Abs(number) < 0.0000001 && number != 0) ||
                   number.ToString(CultureInfo.InvariantCulture).Contains('E');
        }

        private string FormatScientific(double number)
        {
            string result = number.ToString("0.######E+0", CultureInfo.InvariantCulture);
            result = result.Replace(".0E", "E").Replace(".E", "E");
            return result;
        }

        private string FormatRegular(double number)
        {
            string formatted = number.ToString("0.###############", CultureInfo.InvariantCulture);

            if (formatted.Contains('.'))
            {
                formatted = formatted.TrimEnd('0');
                if (formatted.EndsWith("."))
                    formatted = formatted[..^1];
            }

            if (double.IsInteger(number) && Math.Abs(number) < 1_000_000_000)
            {
                formatted = number.ToString("0", CultureInfo.InvariantCulture);
            }

            Debug.WriteLine($"FormatDisplay: {number} → {formatted}");
            return formatted;
        }

        // 1.3 Форматування чисел з дужками для від'ємних значень
        public string FormatResultWithParentheses(double number)
        {
            string formattedNumber = FormatDisplay(number);
            if (!formattedNumber.StartsWith("- ") && number < 0)
            {
                return $"({formattedNumber})";
            }
            return formattedNumber;
        }

        // ==================== РОЗДІЛ 2: ПАРСИНГ ЧИСЕЛ З РЯДКІВ ====================

        public double Parse(string numberStr)
        {
            if (string.IsNullOrEmpty(numberStr))
                return 0;

            string cleanNumber = numberStr.Replace("(", "").Replace(")", "").Trim();

            if (cleanNumber.Length > 0 && "+-*/".Contains(cleanNumber[^1]))
            {
                cleanNumber = cleanNumber[..^1].Trim();
            }

            if (string.IsNullOrEmpty(cleanNumber))
                return 0;

            return double.Parse(cleanNumber, CultureInfo.InvariantCulture);
        }

        // ==================== РОЗДІЛ 3: ФОРМАТУВАННЯ ДЛЯ ІСТОРІЇ ОБЧИСЛЕНЬ ====================

        public string FormatForHistory(string numberStr)
        {
            if (string.IsNullOrEmpty(numberStr))
                return "0";

            if (numberStr.EndsWith('.'))
                numberStr = numberStr[..^1];

            if (numberStr.StartsWith('(') && numberStr.EndsWith(')'))
                return numberStr;

            if (numberStr.EndsWith('%'))
            {
                if (numberStr.StartsWith("(-") && numberStr.EndsWith(")%") || numberStr.EndsWith("%)"))
                    return numberStr;
                return numberStr;
            }

            if (numberStr.StartsWith('-'))
                return $"({numberStr})";

            return numberStr;
        }

        public string FormatExpression(string leftPart, string operation = "", string rightPart = "")
        {
            string formattedLeft = FormatForHistory(leftPart);
            string formattedRight = !string.IsNullOrEmpty(rightPart) ? FormatForHistory(rightPart) : "";
            string displayOperation = operation == "*" ? "×" : operation; // Зміна * на ×

            if (rightPart.EndsWith('%'))
            {
                formattedRight = rightPart;
            }

            return !string.IsNullOrEmpty(displayOperation)
                ? $"{formattedLeft} {displayOperation} {formattedRight} ="
                : $"{formattedLeft} {formattedRight} =";
        }

        // ==================== РОЗДІЛ 4: ФОРМАТУВАННЯ ВІДСОТКІВ ====================

        public string FormatSecondOperand(double number, string? originalInput = null, bool isPercent = false)
        {
            string formatted = FormatDisplay(number);

            if (isPercent)
            {
                return number < 0 ? $"({formatted}%)" : $"{formatted}%";
            }

            if (!string.IsNullOrEmpty(originalInput))
            {
                bool wasParenthesized = originalInput.StartsWith('(') && originalInput.EndsWith(')');
                if (wasParenthesized) return $"({formatted})";
            }

            return number < 0 ? $"({formatted})" : formatted;
        }

        public string ExtractNumberFromPercent(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            if (input.StartsWith("(-") && input.EndsWith("%)"))
            {
                return "-" + input.Substring(2, input.Length - 4);
            }
            else if (input.StartsWith('(') && input.EndsWith("%)"))
            {
                return input.Substring(1, input.Length - 3);
            }
            else if (input.EndsWith('%'))
            {
                return input.Substring(0, input.Length - 1);
            }

            return input;
        }

        public string FormatPercentForHistory(double number)
        {
            string formatted = FormatDisplay(number);
            return number < 0 ? $"({formatted}%)" : $"{formatted}%";
        }

        // ==================== РОЗДІЛ 5: ОПЕРАЦІЇ ЗІ ЗНАКАМИ ====================

        public bool IsNegativeNumber(string input)
        {
            return input.StartsWith('-') || input.StartsWith("(-");
        }

        public string ToggleSign(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            if (input == "0" || input == "(-0)")
                return "0";

            bool isPercent = input.EndsWith('%');

            if (input.StartsWith('-') && isPercent && !input.StartsWith("(-"))
                return input[1..];

            if (input.StartsWith("(-") && isPercent)
                return input[2..^2] + "%";

            if (isPercent)
            {
                string number = input[..^1];
                return $"(-{number}%)";
            }

            if (input.StartsWith("(-") && input.EndsWith(")"))
                return input[2..^1];

            return $"(-{input})";
        }
    }
}