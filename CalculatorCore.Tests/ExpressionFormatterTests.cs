namespace CalculatorCore.Tests
{
    public class ExpressionFormatterTests
    {
        private readonly ExpressionFormatter _formatter;

        public ExpressionFormatterTests()
        {
            _formatter = new ExpressionFormatter();
        }

        // ==================== FormatDisplay ====================
        [Theory]
        [InlineData(123.456, "123.456")]
        [InlineData(-123.456, "-123.456")]
        [InlineData(0.000001, "1E-6")]
        [InlineData(0.0000001, "1E-7")] // Наукова нотація для дуже малих чисел
        [InlineData(double.MaxValue, "1.797693E+308")]
        [InlineData(double.MinValue, "-1.797693E+308")]
        [InlineData(0.00000999999, "9.99999E-6")] // На межі наукової нотації
        [InlineData(999_999_999_999, "1E+12")] // На межі наукової нотації
        [InlineData(0, "0")] // Нуль
        [InlineData(double.PositiveInfinity, "∞")] // Нескінченність
        [InlineData(double.NegativeInfinity, "-∞")] // Від'ємна нескінченність
        [InlineData(double.NaN, "NaN")] // Не число
        [InlineData(1.23E+50, "1.23E+50")] // Вже у науковій нотації
        [InlineData(123.4500, "123.45")] // Видалення зайвих нулів
        [InlineData(123.0, "123")] // Видалення зайвої крапки
        public void FormatDisplay_FormatsNumbersCorrectly(double number, string expected)
        {
            // Act
            string result = _formatter.FormatDisplay(number);

            // Assert
            Assert.Equal(expected, result);
        }

        // ==================== FormatResultWithParentheses ====================
        [Theory]
        [InlineData(5, "5")]
        [InlineData(-5, "(-5)")]
        [InlineData(0, "0")]
        public void FormatResultWithParentheses_AddsParenthesesForNegativeNumbers(double number, string expected)
        {
            // Act
            string result = _formatter.FormatResultWithParentheses(number);

            // Assert
            Assert.Equal(expected, result);
        }

        // ==================== Parse ====================
        [Theory]
        [InlineData("5", 5)]
        [InlineData("(-5)", -5)]
        [InlineData("10 +", 10)] // Обрізає оператор
        [InlineData("", 0)]
        [InlineData("1.5", 1.5)]
        [InlineData(" 123 ", 123)] // Пробіли
        [InlineData("1.23e5", 123000)] // Наукова нотація у вводі
        [InlineData("1.23E-5", 0.0000123)] // Наукова нотація
        //[InlineData(" 123 . 456 ", 123.456)] // Пробіли всередині
        public void Parse_ParsesNumbersCorrectly(string input, double expected)
        {
            // Act
            double result = _formatter.Parse(input);

            // Assert
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("abc")]
        [InlineData("123..456")]
        public void Parse_InvalidInput_ThrowsException(string input)
        {
            Assert.Throws<FormatException>(() => _formatter.Parse(input));
        }

        // ==================== FormatForHistory ====================
        [Theory]
        [InlineData("5", "5")]
        [InlineData("-5", "(-5)")]
        [InlineData("5%", "5%")]
        [InlineData("(-5)%", "(-5)%")]
        [InlineData("", "0")]
        public void FormatForHistory_FormatsCorrectly(string input, string expected)
        {
            // Act
            string result = _formatter.FormatForHistory(input);

            // Assert
            Assert.Equal(expected, result);
        }

        // ==================== FormatExpression ====================
        [Theory]
        [InlineData("5", "+", "3", "5 + 3 =")]
        [InlineData("-5", "*", "3", "(-5) * 3 =")]
        [InlineData("10%", "-", "5%", "10% - 5% =")]
        public void FormatExpression_FormatsCorrectly(string left, string op, string right, string expected)
        {
            // Act
            string result = _formatter.FormatExpression(left, op, right);

            // Assert
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("0", "+", "5", "0 + 5 =")]  // Нульовий лівий операнд
        public void FormatExpression_ZeroLeftPart_HandlesCorrectly(string left, string op, string right, string expected)
        {
            // Act
            string result = _formatter.FormatExpression(left, op, right);

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void FormatExpression_NullLeftPart_DefaultsToZero()
        {
            // Arrange
            string? left = null;
            string op = "+";
            string right = "5";
            string expected = "0 + 5 =";

            // Act
            string result = _formatter.FormatExpression(left!, op, right);

            // Assert
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("5", "", "3", "5 3 =")] // Без оператора
        [InlineData("5", "+", "", "5 +  =")] // Порожній правий операнд
        public void FormatExpression_EdgeCases(string left, string op, string right, string expected)
        {
            // Act
            string result = _formatter.FormatExpression(left, op, right);

            // Assert
            Assert.Equal(expected, result);
        }

        // ==================== FormatSecondOperand ====================
        [Theory]
        [InlineData(5, null, false, "5")] // Звичайне число
        [InlineData(-5, null, false, "(-5)")] // Від'ємне число
        [InlineData(5, "5%", true, "5%")] // Відсоток
        [InlineData(-5, "(-5%)", true, "(-5%)")] // Відсоток у дужках
        public void FormatSecondOperand_FormatsCorrectly(double number, string? originalInput, bool isPercent, string expected)
        {
            // Act
            string result = _formatter.FormatSecondOperand(number, originalInput, isPercent);

            // Assert
            Assert.Equal(expected, result);
        }

        // ==================== ToggleSign ====================
        [Theory]
        [InlineData("5", "(-5)")] // Додатнє → від'ємне
        [InlineData("(-5)", "5")] // Від'ємне → додатнє
        [InlineData("5%", "(-5%)")] // Відсоток → від'ємний відсоток
        [InlineData("(-5%)", "5%")] // Від'ємний відсоток → додатній
        [InlineData("5.5%", "(-5.5%)")] // Дроби у відсотках
        [InlineData("0", "0")] // Нуль залишається нулем
        [InlineData("(-0)", "0")] // Особливий випадок нуля
        [InlineData("", "")] // Порожній ввід
        public void ToggleSign_TogglesCorrectly(string input, string expected)
        {
            // Act
            string result = _formatter.ToggleSign(input);

            // Assert
            Assert.Equal(expected, result);
        }

        // ==================== IsNegativeNumber ====================
        [Theory]
        [InlineData("-5", true)]      // Від'ємне число
        [InlineData("(-5)", true)]    // Від'ємне число у дужках
        [InlineData("5", false)]      // Додатнє число
        [InlineData("0", false)]      // Нуль
        public void IsNegativeNumber_DetectsCorrectly(string input, bool expected)
        {
            // Act
            bool result = _formatter.IsNegativeNumber(input);

            // Assert
            Assert.Equal(expected, result);
        }

        // ==================== ExtractNumberFromPercent ====================
        [Theory]
        [InlineData("(-10%)", "-10")]    // Від'ємний відсоток у дужках
        [InlineData("(10%)", "10")]      // Додатній відсоток у дужках
        [InlineData("10%", "10")]        // Звичайний відсоток без дужок
        [InlineData("abc", "abc")]       // Не відсоток (повертає як є)
        [InlineData("", "")]             // Порожній рядок
        [InlineData("(5.5%)", "5.5")]    // Дробовий відсоток у дужках
        [InlineData("1000%", "1000")]    // Великий відсоток
        [InlineData("(-10.5%)", "-10.5")] // Від'ємний дробовий відсоток
        public void ExtractNumberFromPercent_ExtractsCorrectly(string input, string expected)
        {
            // Act
            string result = _formatter.ExtractNumberFromPercent(input);

            // Assert
            Assert.Equal(expected, result);
        }

        // ==================== FormatPercentForHistory ====================
        [Theory]
        [InlineData(10)]
        [InlineData(-10)]
        [InlineData(0)]
        [InlineData(5.5)]
        public void FormatPercentForHistory_WithExtractedValues(double number)
        {
            // Arrange
            string formatted = _formatter.FormatPercentForHistory(number);
            string extracted = _formatter.ExtractNumberFromPercent(formatted);
            double parsed = _formatter.Parse(extracted);

            // Assert
            Assert.Equal(number, parsed);
        }

        [Theory]
        [InlineData(5, "5%")]         // Додатнє число
        [InlineData(-5, "(-5%)")]     // Від'ємне число
        public void FormatPercentForHistory_FormatsCorrectly(double number, string expected)
        {
            // Act
            string result = _formatter.FormatPercentForHistory(number);

            // Assert
            Assert.Equal(expected, result);
        }

        // ==================== Integration with Parse ====================
        [Theory]
        [InlineData("10%", 10)]
        [InlineData("(-10%)", -10)]
        [InlineData("5.5%", 5.5)]
        public void ExtractAndParse_WorksCorrectly(string input, double expected)
        {
            // Act
            string extracted = _formatter.ExtractNumberFromPercent(input);
            double result = _formatter.Parse(extracted);

            // Assert
            Assert.Equal(expected, result);
        }
    }
}