namespace CalculatorCore.Tests
{
    public class InputHandlerTests
    {
        private readonly CalculatorEngine _calculator;
        private readonly ExpressionFormatter _formatter;
        private readonly InputHandler _inputHandler;

        public InputHandlerTests()
        {
            _calculator = new CalculatorEngine();
            _formatter = new ExpressionFormatter();
            _inputHandler = new InputHandler(_calculator, _formatter);
        }

        // ============= Стани та базові операції =============
        [Fact]
        public void InitialState_IsCorrect()
        {
            Assert.Equal("", _inputHandler.CurrentInput);
            Assert.Equal("", _inputHandler.FullExpression);
            Assert.True(_inputHandler.IsNewInput);
            Assert.False(_inputHandler.ErrorState);
        }

        [Fact]
        public void HandleDigit_StartsNewInput()
        {
            _inputHandler.HandleDigit("5");
            Assert.Equal("5", _inputHandler.CurrentInput);
            Assert.False(_inputHandler.IsNewInput);
        }

        // ============= Обробка операторів =============
        [Theory]
        [InlineData('+')]
        [InlineData('-')]
        [InlineData('×')]
        [InlineData('/')]
        public void HandleOperator_WithCurrentInput_UpdatesExpression(char op)
        {
            // Arrange
            _inputHandler.HandleDigit("5");

            // Act
            _inputHandler.HandleOperator(op);

            // Assert
            Assert.Equal($"5 {op} ", _inputHandler.FullExpression);
            Assert.True(_inputHandler.IsNewInput);
        }

        // ============= Спеціальні функції =============
        [Fact]
        public void HandleToggleSign_TogglesCurrentInput()
        {
            // Arrange
            _inputHandler.HandleDigit("5");

            // Act
            _inputHandler.HandleToggleSign();

            // Assert
            Assert.Equal("(-5)", _inputHandler.CurrentInput);

            // Act - toggle back
            _inputHandler.HandleToggleSign();

            // Assert
            Assert.Equal("5", _inputHandler.CurrentInput);
        }

        // ============= Обчислення =============
        [Fact]
        public void HandleEquals_WithSimpleAddition_ProcessesCalculation()
        {
            // Arrange
            _inputHandler.HandleDigit("5");
            _inputHandler.HandleOperator('+');
            _inputHandler.HandleDigit("3");

            // Act
            _inputHandler.HandleEquals();

            // Assert
            Assert.Equal("8", _inputHandler.CurrentInput);
            Assert.Contains("=", _inputHandler.FullExpression);
        }

        // ============= Відсотки =============
        [Fact]
        public void HandlePercent_StandalonePercent_CalculatesCorrectly()
        {
            // Arrange
            _inputHandler.HandleDigit("5");

            // Act
            _inputHandler.HandlePercent();
            _inputHandler.HandleEquals();

            // Assert
            Assert.Equal("0.05", _inputHandler.CurrentInput);
        }

        // ============= Скидання =============
        [Fact]
        public void HandleClear_FullClear_ResetsState()
        {
            // Arrange
            _inputHandler.HandleDigit("5");
            _inputHandler.HandleOperator('+');

            // Act
            _inputHandler.HandleClear(true);

            // Assert
            Assert.Equal("", _inputHandler.CurrentInput);
            Assert.Equal("", _inputHandler.FullExpression);
            Assert.True(_inputHandler.IsNewInput);
        }

        [Fact]
        public void HandleClear_PartialClear_RemovesLastDigit()
        {
            // Arrange
            _inputHandler.HandleDigit("5");
            _inputHandler.HandleDigit("3");

            // Act
            _inputHandler.HandleClear(false);

            // Assert
            Assert.Equal("5", _inputHandler.CurrentInput);
        }

        // ============= Додаткові тести для покриття =============
        [Fact]
        public void HandleOperator_AfterEquals_UsesResult()
        {
            // Arrange
            _inputHandler.HandleDigit("5");
            _inputHandler.HandleOperator('+');
            _inputHandler.HandleDigit("3");
            _inputHandler.HandleEquals();

            // Act
            _inputHandler.HandleOperator('-');

            // Assert
            Assert.Equal("8 - ", _inputHandler.FullExpression);
        }

        [Fact]
        public void HandleDigit_AfterEquals_StartsNewInput()
        {
            // Arrange
            _inputHandler.HandleDigit("5");
            _inputHandler.HandleOperator('+');
            _inputHandler.HandleDigit("3");
            _inputHandler.HandleEquals();

            // Act
            _inputHandler.HandleDigit("2");

            // Assert
            Assert.Equal("2", _inputHandler.CurrentInput);
            Assert.Equal("", _inputHandler.FullExpression);
        }

        [Fact]
        public void HandlePercent_AfterDecimalPoint_RemovesPoint()
        {
            _inputHandler.HandleDigit("5");
            _inputHandler.HandleDecimalPoint();
            _inputHandler.HandlePercent();

            Assert.Equal("5%", _inputHandler.CurrentInput);
            Assert.False(_inputHandler.ErrorState);
        }
    }
}
