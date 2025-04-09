namespace CalculatorCore.Tests
{
    public class SignToggleTests : CalculatorTestBase
    {
        public SignToggleTests() : base(useRealEngine: false)
        {
        }

        [Theory]
        [InlineData("5", "(-5)")]    // Додаємо мінус
        [InlineData("(-3)", "3")]    // Прибираємо мінус
        [InlineData("0", "0")]     // Нуль залишається нулем
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

            // Act
            InputHandler.HandleToggleSign(); // 5 → -5
            InputHandler.HandleToggleSign(); // -5 → 5

            // Assert
            Assert.Equal("5", InputHandler.CurrentInput);
        }

        [Fact]
        public void ToggleSign_AfterOperator_AppliesToNextNumber()
        {
            // Arrange
            InputHandler.HandleDigit("1");
            InputHandler.HandleDigit("0");
            InputHandler.HandleOperator('+');

            // Перевірка стану після оператора
            Assert.Equal("10 + ", InputHandler.FullExpression);
            Assert.Equal("", InputHandler.CurrentInput);
            Assert.True(InputHandler.IsNewInput);

            // Act 1 - Toggle sign
            InputHandler.HandleToggleSign();
            Assert.Equal("-", InputHandler.CurrentInput);

            // Act 2 - Enter 5
            InputHandler.HandleDigit("5");
            Assert.Equal("-5", InputHandler.CurrentInput);

            // Act 3 - Calculate
            InputHandler.HandleEquals();

            // Assert
            Assert.Equal("5", InputHandler.CurrentInput);

            // Очікуємо "10 + (-5) =" або "10 + -5 =", залежно від форматера
            Assert.True(InputHandler.FullExpression == "10 + (-5) =" ||
                        InputHandler.FullExpression == "10 + -5 =");
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