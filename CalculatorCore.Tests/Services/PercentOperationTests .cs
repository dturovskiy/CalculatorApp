namespace CalculatorCore.Tests
{
    public class PercentOperationTests : CalculatorTestBase
    {
        public PercentOperationTests() : base(useRealEngine: false)
        {
        }

        [Fact]
        public void HandlePercent_AddsPercentSymbol()
        {
            // Arrange
            EnterDigits("50");

            // Act
            InputHandler.HandlePercent();

            // Assert
            Assert.Equal("50%", InputHandler.CurrentInput);
        }

        [Fact]
        public void HandleEquals_WithSimplePercent_CalculatesCorrectly()
        {
            // Arrange
            SetupPercentCalculation(50, 0.5);
            EnterDigits("50");
            InputHandler.HandlePercent();

            // Act
            InputHandler.HandleEquals();

            // Assert
            Assert.Equal("0.5", InputHandler.CurrentInput);
            Assert.Equal("50% =", InputHandler.FullExpression);
        }

        [Fact]
        public void HandleEquals_WithOperationPercent_CalculatesCorrectly()
        {
            // Arrange
            SetupPercentCalculation(10, 110, '+');
            EnterExpression("100+10");
            InputHandler.HandlePercent();

            // Act
            InputHandler.HandleEquals();

            // Assert
            Assert.Equal("110", InputHandler.CurrentInput);
            Assert.Equal("100 + 10% =", InputHandler.FullExpression);
        }

        [Fact]
        public void HandlePercent_AfterOperator_DoesNotAffectExpression()
        {
            // Arrange
            EnterExpression("50+10");

            // Act
            InputHandler.HandlePercent();

            // Assert
            Assert.Equal("10%", InputHandler.CurrentInput);
            Assert.Equal("50 + ", InputHandler.FullExpression);
        }

        [Fact]
        public void HandlePercent_OnEmptyInput_DoesNothing()
        {
            // Act
            InputHandler.HandlePercent();

            // Assert
            Assert.Equal("", InputHandler.CurrentInput);
        }

        [Fact]
        public void HandlePercent_MultiplePercentSigns_OnlyFirstIsProcessed()
        {
            // Arrange
            EnterDigits("50");
            InputHandler.HandlePercent();

            // Act
            InputHandler.HandlePercent(); // другий знак % ігнорується

            // Assert
            Assert.Equal("50%", InputHandler.CurrentInput);
        }
    }
}