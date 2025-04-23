namespace CalculatorCore.Tests
{
    public class DecimalOperationsTests : CalculatorTestBase
    {
        public DecimalOperationsTests() : base(useRealEngine: true)
        {
        }

        [Fact]
        public void HandleDecimalPoint_AddsDecimalToNumber()
        {
            // Act
            EnterDigits("5");
            InputHandler.HandleDecimalPoint();
            EnterDigits("25");

            // Assert
            Assert.Equal("5.25", InputHandler.CurrentInput);
        }

        [Fact]
        public void HandleDecimalPoint_IgnoresSecondPointInNumber()
        {
            // Arrange
            EnterDigits("3.14");

            // Act
            InputHandler.HandleDecimalPoint(); // Спроба додати другу крапку
            EnterDigits("15");

            // Assert
            Assert.Equal("3.1415", InputHandler.CurrentInput); // Крапка не додалась
        }

        [Fact]
        public void HandleDecimalPoint_AfterOperator_StartsNewDecimalNumber()
        {
            // Arrange
            EnterExpression("10+");

            // Act
            InputHandler.HandleDecimalPoint();
            EnterDigits("5");

            // Assert
            Assert.Equal("0.5", InputHandler.CurrentInput); // Починає нове число
            Assert.Equal("", InputHandler.FullExpression);
        }

        [Fact]
        public void Calculate_WithDecimalNumbers_ReturnsCorrectResult()
        {
            // Імітуємо реальний UI (без EnterDigits)
            InputHandler.HandleDigit("2");
            InputHandler.HandleDecimalPoint();
            InputHandler.HandleDigit("5");
            InputHandler.HandleOperator('+');
            InputHandler.HandleDigit("2");
            InputHandler.HandleDecimalPoint();
            InputHandler.HandleDigit("5");
            InputHandler.HandleEquals();

            Assert.Equal("5", InputHandler.CurrentInput);
        }

        [Fact]
        public void HandleDecimalPoint_OnEmptyInput_StartsWithZero()
        {
            // Act
            InputHandler.HandleDecimalPoint();
            EnterDigits("5");

            // Assert
            Assert.Equal("0.5", InputHandler.CurrentInput);
        }
    }
}