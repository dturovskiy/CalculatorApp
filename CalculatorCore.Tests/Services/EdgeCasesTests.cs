namespace CalculatorCore.Tests
{
    public class EdgeCasesTests : CalculatorTestBase
    {
        public EdgeCasesTests() : base(useRealEngine: true) // Використовуємо реальний двигун для складних кейсів
        {
        }

        // ===== Тести для ділення на нуль =====
        [Fact]
        public void DivisionByZero_ShowsError()
        {
            // Arrange
            EnterExpression("5/0");

            // Act
            InputHandler.HandleEquals();

            // Assert
            Assert.Equal("ERROR", InputHandler.CurrentInput);
            Assert.True(InputHandler.ErrorState);
        }

        // ===== Тести для великих чисел =====
        [Fact]
        public void LargeNumberCalculation_ReturnsScientificNotation()
        {
            // Arrange
            EnterDigits("999999999999");
            InputHandler.HandleOperator('*');
            EnterDigits("999999999999");

            // Act
            InputHandler.HandleEquals();

            // Assert
            Assert.Equal("9.99999999998E+23", InputHandler.CurrentInput);
        }

        [Fact]
        public void Equals_IsIdempotent()
        {
            // Arrange
            EnterExpression("10+5");
            InputHandler.HandleEquals(); // Перше обчислення: 15

            // Act
            InputHandler.HandleEquals(); // Повинно ігноруватись
            InputHandler.HandleEquals(); // Повинно ігноруватись

            // Assert
            Assert.Equal("15", InputHandler.CurrentInput);
            Assert.Equal("10 + 5 =", InputHandler.FullExpression);
        }

        // ===== Тести для некоректних вводів =====
        [Fact]
        public void MultipleOperators_KeepsLastOperator()
        {
            // Arrange
            EnterExpression("10+-*/");

            // Act
            InputHandler.HandleDigit("5");
            InputHandler.HandleEquals();

            // Assert
            Assert.Equal("2", InputHandler.CurrentInput); // 10 / 5 = 2
            Assert.Equal("10 / 5 =", InputHandler.FullExpression);
        }

        // ===== Тести для "0.000..." =====
        [Fact]
        public void MultipleLeadingZeros_SimplifiesToSingleZero()
        {
            // Arrange
            EnterDigits("000000.5");

            // Assert
            Assert.Equal("0.5", InputHandler.CurrentInput);
        }

        [Fact]
        public void VerySmallNumber_DisplaysCorrectly()
        {
            // Arrange
            EnterDigits("0.000000001");

            // Assert
            Assert.Equal("0.000000001", InputHandler.CurrentInput);
        }
    }
}