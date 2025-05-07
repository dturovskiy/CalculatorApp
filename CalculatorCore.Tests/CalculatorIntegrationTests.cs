namespace CalculatorCore.Tests
{
    public class CalculatorIntegrationTests
    {
        private readonly CalculatorEngine _engine;
        private readonly ExpressionFormatter _formatter;
        private readonly InputHandler _inputHandler;

        public CalculatorIntegrationTests()
        {
            _engine = new CalculatorEngine();
            _formatter = new ExpressionFormatter();
            _inputHandler = new InputHandler(_engine, _formatter);
        }

        // ============= Basic Arithmetic Operations =============
        [Fact]
        public void FullFlow_SimpleAddition_ReturnsCorrectResult()
        {
            // Введення: 5 + 3 =
            _inputHandler.HandleDigit("5");
            _inputHandler.HandleOperator('+');
            _inputHandler.HandleDigit("3");
            _inputHandler.HandleEquals();

            Assert.Equal("8", _inputHandler.CurrentInput);
            Assert.Equal("5 + 3 =", _inputHandler.FullExpression);
        }

        // ============= Percent Calculations =============
        [Fact]
        public void FullFlow_WithPercent_CalculatesCorrectly()
        {
            // Введення: 100 + 10% =
            _inputHandler.HandleDigit("1");
            _inputHandler.HandleDigit("0");
            _inputHandler.HandleDigit("0");
            _inputHandler.HandleOperator('+');
            _inputHandler.HandleDigit("1");
            _inputHandler.HandleDigit("0");
            _inputHandler.HandlePercent();
            _inputHandler.HandleEquals();

            Assert.Equal("110", _inputHandler.CurrentInput);
            Assert.Equal("100 + 10% =", _inputHandler.FullExpression);
        }

        [Fact]
        public void FullFlow_PercentOfNumber_CalculatesCorrectly()
        {
            // Введення: 20% від 50 = 10
            _inputHandler.HandleDigit("2");
            _inputHandler.HandleDigit("0");
            _inputHandler.HandlePercent();
            _inputHandler.HandleDigit("5");
            _inputHandler.HandleDigit("0");
            _inputHandler.HandleEquals();

            Assert.Equal("10", _inputHandler.CurrentInput);
            Assert.Contains("20% 50 =", _inputHandler.FullExpression);
        }

        // ============= Error Handling =============
        [Fact]
        public void FullFlow_DivideByZero_ShowsError()
        {
            // Введення: 5 / 0 =
            _inputHandler.HandleDigit("5");
            _inputHandler.HandleOperator('/');
            _inputHandler.HandleDigit("0");
            _inputHandler.HandleEquals();

            Assert.Equal("ERROR", _inputHandler.CurrentInput);
            Assert.True(_inputHandler.ErrorState);
        }

        [Fact]
        public void FullFlow_DecimalPointInFirstOperand_RemovesPointFromHistory()
        {
            // Введення: 5. + 3 =
            _inputHandler.HandleDigit("5");
            _inputHandler.HandleDecimalPoint();
            _inputHandler.HandleOperator('+');
            _inputHandler.HandleDigit("3");
            _inputHandler.HandleEquals();

            Assert.Equal("8", _inputHandler.CurrentInput);
            Assert.Equal("5 + 3 =", _inputHandler.FullExpression); // Крапки немає
        }

        [Fact]
        public void Backspace_ResetsDecimalPointFlag()
        {
            _inputHandler.HandleDigit("5");
            _inputHandler.HandleDecimalPoint(); // "5."
            _inputHandler.HandleClear(false);   // Backspace
            _inputHandler.HandleDecimalPoint(); // Має дозволити крапку знову

            Assert.Equal("5.", _inputHandler.CurrentInput);
        }
    }
}