namespace CalculatorCore.Tests
{
    public class SignToggleTests : CalculatorTestBase
    {
        public SignToggleTests() : base(useRealEngine: true) // Використовуємо реальний двигун і форматер
        {
        }

        [Theory]
        [InlineData("5", "(-5)")]    // Додаємо мінус і дужки
        [InlineData("3", "(-3)")]    // Прибираємо мінус і дужки
        [InlineData("0", "0")]       // Нуль залишається нулем
        public void ToggleSign_ChangesNumberSign(string input, string expected)
        {
            // Arrange
            EnterDigits(input);

            // Act
            InputHandler.HandleToggleSign();

            // Assert
            Assert.Equal(expected, InputHandler.CurrentInput);
        }

        [Fact]
        public void ToggleSign_Twice_RevertsToOriginal()
        {
            // Arrange
            EnterDigits("5");

            // Act & Assert
            InputHandler.HandleToggleSign();
            Assert.Equal("(-5)", InputHandler.CurrentInput);

            InputHandler.HandleToggleSign();
            Assert.Equal("5", InputHandler.CurrentInput);
        }

        [Fact]
        public void ToggleSignTest_AfterDecimalPoint()
        {
            // Вводимо "0.5" → "-0.5" → знову "0.5"
            EnterDigits("0.5");
            InputHandler.HandleToggleSign();
            Assert.Equal("(-0.5)", InputHandler.CurrentInput);

            InputHandler.HandleToggleSign();
            Assert.Equal("0.5", InputHandler.CurrentInput);
        }

        [Fact]
        public void ToggleSign_AfterOperator_CreatesNegativeNumber()
        {
            // Arrange
            InputHandler.HandleDigit("1");
            InputHandler.HandleDigit("0");
            InputHandler.HandleOperator('+');

            // Act
            InputHandler.HandleDigit("5");
            InputHandler.HandleToggleSign();
            InputHandler.HandleEquals();

            // Assert
            Assert.Equal("5", InputHandler.CurrentInput);
            Assert.Equal("10 + (-5) =", InputHandler.FullExpression);
        }

        [Fact]
        public void ToggleSign_OnEmptyInput_DoesNothing()
        {
            // Act
            InputHandler.HandleToggleSign();

            // Assert
            Assert.Equal("", InputHandler.CurrentInput);
        }
    }
}