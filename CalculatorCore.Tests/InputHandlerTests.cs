namespace CalculatorCore.Tests
{
    public class InputHandlerTests
    {
        private readonly InputHandler _handler = new();

        // 1. Початковий стан
        [Fact]
        public void Constructor_InitializesEmptyState()
        {
            Assert.Equal("", _handler.CurrentInput);
            Assert.Equal("", _handler.FullExpression);
            Assert.True(_handler.IsNewInput);
        }

        // 2. Введення цифр
        [Theory]
        [InlineData("5", "5")]
        [InlineData("0", "0")]
        public void HandleDigit_SingleDigit_SetsCurrentInput(string digit, string expected)
        {
            _handler.HandleDigit(digit);
            Assert.Equal(expected, _handler.CurrentInput);
            Assert.False(_handler.IsNewInput);
        }

        // 3. Видалення символів (←)
        [Fact]
        public void HandleBackspace_WithDigits_RemovesLastChar()
        {
            _handler.HandleDigit("1");
            _handler.HandleDigit("2");
            _handler.HandleDigit("3");
            _handler.HandleClear(false); // Симулюємо ←

            Assert.Equal("12", _handler.CurrentInput);
        }

        [Fact]
        public void HandleBackspace_LastDigit_ResetsToNewInput()
        {
            _handler.HandleDigit("5");
            _handler.HandleClear(false); // Симулюємо ←

            Assert.Equal("", _handler.CurrentInput);
            Assert.True(_handler.IsNewInput);
        }

        // 4. Повне очищення (AC)
        [Fact]
        public void HandleClear_FullClear_ResetsAllState()
        {
            _handler.HandleDigit("5");
            _handler.HandleOperator('+');
            _handler.HandleClear(true); // AC

            Assert.Equal("", _handler.CurrentInput);
            Assert.Equal("", _handler.FullExpression);
        }

        // 5. Десяткова крапка
        [Fact]
        public void HandleDecimalPoint_FirstInput_StartsWithZeroPoint()
        {
            _handler.HandleDecimalPoint();
            Assert.Equal("0.", _handler.CurrentInput);
        }

        [Fact]
        public void HandleDecimalPoint_Duplicate_IgnoresSecondPoint()
        {
            _handler.HandleDigit("5");
            _handler.HandleDecimalPoint();
            _handler.HandleDecimalPoint(); // Повинен ігноруватись

            Assert.Equal("5.", _handler.CurrentInput);
        }

        // 6. Від'ємні числа (+/-)
        [Fact]
        public void HandleToggleSign_TogglesPositiveToNegative()
        {
            _handler.HandleDigit("5");
            _handler.HandleToggleSign();
            Assert.Equal("-5", _handler.CurrentInput);
        }

        [Fact]
        public void HandleToggleSign_WithDecimal_HandlesCorrectly()
        {
            _handler.HandleDigit("0");
            _handler.HandleDecimalPoint();
            _handler.HandleToggleSign();
            Assert.Equal("-0.", _handler.CurrentInput);
        }

        // 7. Оператори
        [Fact]
        public void HandleOperator_AfterNumber_AddsToExpression()
        {
            _handler.HandleDigit("5");
            _handler.HandleOperator('+');

            Assert.Equal("5 + ", _handler.FullExpression);
            Assert.Equal("", _handler.CurrentInput);
        }

        [Fact]
        public void HandleOperator_ChangeOperator_UpdatesExpression()
        {
            _handler.HandleDigit("5");
            _handler.HandleOperator('+');
            _handler.HandleOperator('-');

            Assert.Equal("5 - ", _handler.FullExpression);
        }

        // 8. Дужки в історії (тестується через HandleEquals)
        [Fact]
        public void HandleEquals_NegativeNumber_WrapsInParentheses()
        {
            _handler.HandleDigit("5");
            _handler.HandleOperator('+');
            _handler.HandleDigit("3");
            _handler.HandleToggleSign();
            _handler.HandleEquals();

            Assert.Contains("(-3)", _handler.FullExpression);
        }

        // 9. Порожній "="
        [Fact]
        public void HandleEquals_WithoutExpression_NoChanges()
        {
            var initialState = _handler.FullExpression;
            _handler.HandleEquals();
            Assert.Equal(initialState, _handler.FullExpression);
        }

        // 10. Крапка після оператора
        [Fact]
        public void HandleDecimalPoint_AfterOperator_StartsNewDecimal()
        {
            _handler.HandleDigit("5");
            _handler.HandleOperator('+');
            _handler.HandleDecimalPoint();

            Assert.Equal("0.", _handler.CurrentInput);
        }

        // 11. Після "="
        [Fact]
        public void HandleDigit_AfterEquals_StartsNewInput()
        {
            _handler.HandleDigit("5");
            _handler.HandleOperator('+');
            _handler.HandleDigit("3");
            _handler.HandleEquals();
            _handler.HandleDigit("2");

            Assert.Equal("2", _handler.CurrentInput);
            Assert.Equal("", _handler.FullExpression);
        }

        // 12. Edge-випадки
        [Fact]
        public void HandleDigit_LongNumber_NoOverflow()
        {
            for (int i = 0; i < 20; i++)
            {
                _handler.HandleDigit("9");
            }

            Assert.Equal(20, _handler.CurrentInput.Length);
        }

        [Fact]
        public void HandleOperator_WithoutNumber_Ignores()
        {
            var initialState = _handler.FullExpression;
            _handler.HandleOperator('+');
            Assert.Equal(initialState, _handler.FullExpression);
        }
    }
}