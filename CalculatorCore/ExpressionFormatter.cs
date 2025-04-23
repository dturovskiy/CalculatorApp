using System.Diagnostics;
using System.Globalization;

namespace CalculatorCore
{
    public class ExpressionFormatter
    {
        // 1. Метод для форматування чисел (на вивід)
        public string FormatDisplay(double number)
        {
            // Якщо число дуже велике або дуже мале, використовуємо наукову нотацію
            if (Math.Abs(number) >= 1_000_000_000_000 || (Math.Abs(number) < 0.0001 && number != 0))
            {
                return number.ToString("0.##############E+0", CultureInfo.InvariantCulture);
            }

            // Для звичайних чисел використовуємо поточний формат
            return number.ToString("0.###############", CultureInfo.InvariantCulture);
        }

        public string FormatResultWithParentheses(double number)
        {
            string formattedNumber = FormatDisplay(number); // Спочатку форматуємо число

            if (!formattedNumber.StartsWith("- ") && number < 0)
            {
                return $"({formattedNumber})"; // Додаємо дужки для від'ємних
            }

            return formattedNumber; // Без змін для додатних
        }

        // 2. Метод для парсингу чисел (з рядка)
        public double Parse(string numberStr)
        {
            if (string.IsNullOrEmpty(numberStr))
                return 0;

            // Видаляємо дужки для від'ємних чисел
            string cleanNumber = numberStr.Replace("(", "").Replace(")", "").Trim();

            // Якщо рядок закінчується на оператор (наприклад, "10 +"), видаляємо його
            if (cleanNumber.Length > 0 && "+-*/".Contains(cleanNumber[^1]))
            {
                cleanNumber = cleanNumber[..^1].Trim();
            }

            if (string.IsNullOrEmpty(cleanNumber))
                return 0;

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
            if (numberStr.EndsWith('%'))
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

            if (rightPart.EndsWith('%'))
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
                bool wasParenthesized = originalInput.StartsWith('(') && originalInput.EndsWith(')');
                if (wasParenthesized) return $"({formatted})";
            }

            // 3. Стандартне форматування
            return number < 0 ? $"({formatted})" : formatted;
        }

        public bool IsPercentWithParentheses(string input)
        {
            return input.Contains(")%")
                   && input.StartsWith('(')
                   && input.IndexOf(')') == input.Length - 2;
        }

        public bool IsNegativePercent(string input)
        {
            return input.StartsWith('-') && input.EndsWith('%');
        }

        public bool IsParenthesizedNegativePercent(string input)
        {
            return input.StartsWith("(-") && input.EndsWith(")%");
        }

        public string ExtractNumberFromPercent(string input)
        {
            if (IsParenthesizedNegativePercent(input))
                return input.TrimStart('(').TrimEnd(')', '%');

            if (IsNegativePercent(input) || input.EndsWith('%'))
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
            Debug.WriteLine($"Input: '{input}'");

            if (string.IsNullOrEmpty(input))
                return input;

            // Якщо введення "0" або "(-0)", повертаємо "0"
            if (input == "0" || input == "(-0)")
                return "0";

            bool isPercent = input.EndsWith('%');

            // Випадок: -5% → 5%
            if (input.StartsWith('-') && isPercent && !input.StartsWith("(-"))
                return input[1..]; // знімає лише перший мінус

            // Випадок: (-5%) → 5%
            if (input.StartsWith("(-") && isPercent)
                return input[2..^2] + "%"; // знімає "(-" і ")" (але додає % назад)

            // Випадок: 5% → (-5)%
            if (isPercent)
            {
                string number = input[..^1]; // без '%'
                return $"(-{number})%";
            }

            // Випадок: (-5) → 5
            if (input.StartsWith("(-") && input.EndsWith(")"))
                return input[2..^1]; // знімає дужки

            // Випадок: 5 → (-5)
            return $"(-{input})";
        }

    }
}
