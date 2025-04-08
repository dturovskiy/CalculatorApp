using System.Globalization;

namespace CalculatorCore
{
    public class ExpressionFormatter
    {
        // 1. Метод для форматування чисел (на вивід)
        public string FormatDisplay(double number)
        {
            return number.ToString("0.###############", CultureInfo.InvariantCulture);
        }

        // 2. Метод для парсингу чисел (з рядка)
        public double Parse(string numberStr)
        {
            if (string.IsNullOrEmpty(numberStr))
                return 0;

            // Видаляємо дужки для від'ємних чисел
            string cleanNumber = numberStr.Replace("(", "").Replace(")", "");
            return double.Parse(cleanNumber, CultureInfo.InvariantCulture);
        }

        public string FormatForHistory(string numberStr)
        {
            if (string.IsNullOrEmpty(numberStr))
                return "0";

            // Якщо вже маємо коректні дужки (від'ємне число або відсоток у дужках)
            if (numberStr.StartsWith('(') && numberStr.EndsWith(')'))
                return numberStr;

            // Спеціальна обробка для відсотків
            if (numberStr.EndsWith("%"))
            {
                // Для відсотків у дужках: (-5%) → залишаємо як є
                if (numberStr.StartsWith("(-") && numberStr.EndsWith(")%"))
                    return numberStr;

                // Для звичайних відсотків: -5% → залишаємо без дужок
                return numberStr;
            }

            // Для звичайних від'ємних чисел: -5 → (-5)
            if (numberStr.StartsWith('-'))
                return $"({numberStr})";

            return numberStr;
        }

        public string FormatExpression(string leftPart, string operation = "", string rightPart = "")
        {
            string formattedLeft = FormatForHistory(leftPart);
            string formattedRight = !string.IsNullOrEmpty(rightPart) ? FormatForHistory(rightPart) : "";

            if (rightPart.EndsWith("%"))
            {
                formattedRight = rightPart; // Не змінюємо відсотки
            }

            return !string.IsNullOrEmpty(operation)
                ? $"{formattedLeft} {operation} {formattedRight} ="
                : $"{formattedLeft}{formattedRight} =";
        }

        public string FormatSecondOperand(double number, string? originalInput = null, bool isPercent = false)
        {
            string formatted = FormatDisplay(number);

            // 1. Обробка відсотків (пріоритетна)
            if (isPercent)
            {
                return number < 0 ? $"({formatted}%)" : $"{formatted}%";
            }

            // 2. Збереження оригінального формату (якщо передано)
            if (!string.IsNullOrEmpty(originalInput))
            {
                bool wasParenthesized = originalInput.StartsWith("(") && originalInput.EndsWith(")");
                if (wasParenthesized) return $"({formatted})";
            }

            // 3. Стандартне форматування
            return number < 0 ? $"({formatted})" : formatted;
        }

        public bool IsPercentWithParentheses(string input)
        {
            return input.Contains(")%")
                   && input.StartsWith("(")
                   && input.IndexOf(')') == input.Length - 2;
        }

        public bool IsNegativePercent(string input)
        {
            return input.StartsWith("-") && input.EndsWith("%");
        }

        public bool IsParenthesizedNegativePercent(string input)
        {
            return input.StartsWith("(-") && input.EndsWith(")%");
        }

        public string ExtractNumberFromPercent(string input)
        {
            if (IsParenthesizedNegativePercent(input))
                return input.TrimStart('(').TrimEnd(')', '%');

            if (IsNegativePercent(input) || input.EndsWith("%"))
                return input.TrimEnd('%');

            return input;
        }

        public string FormatPercentForHistory(double number)
        {
            return number < 0 ? $"({FormatDisplay(number)}%)" : $"{FormatDisplay(number)}%";
        }

        public bool IsNegativeNumber(string input)
        {
            return input.StartsWith('-') || input.StartsWith("(-");
        }

        public string ToggleSign(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;

            if (IsNegativePercent(input))
                return input.Substring(1);

            if (IsParenthesizedNegativePercent(input))
                return input.TrimStart('(').TrimEnd(')', '%') + "%";

            if (input.EndsWith("%"))
                return $"-{input}";

            if (IsPercentWithParentheses(input))  // Обробляє (-5%) і (-5)%
            {
                string number = input.TrimStart('(').TrimEnd(')', '%');
                return number.StartsWith("-")
                    ? $"{number.Substring(1)}%"
                    : $"(-{number})%";
            }

            return IsNegativeNumber(input)
                ? input.TrimStart('-').TrimStart('(').TrimEnd(')')
                : $"(-{input})";
        }
    }
}
