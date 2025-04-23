using Moq;
using System.Globalization;

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

        [Theory]
        [InlineData("50", 0.5, "50% =")]
        [InlineData("12.5", 0.125, "12.5% =")]
        public void HandleEquals_WithSimplePercent_CalculatesCorrectly(string input, double result, string expectedExpression)
        {
            // Arrange
            var invariantCulture = CultureInfo.InvariantCulture;
            SetupPercentCalculation(double.Parse(input, invariantCulture), result);

            // Вводимо цифри з явною обробкою крапки
            foreach (var c in input)
            {
                if (c == '.')
                    InputHandler.HandleDecimalPoint();
                else
                    InputHandler.HandleDigit(c.ToString());
            }

            InputHandler.HandlePercent();

            // Act
            InputHandler.HandleEquals();

            // Assert
            Assert.Equal(result.ToString(invariantCulture), InputHandler.CurrentInput); // Використовуємо InvariantCulture
            Assert.Equal(expectedExpression, InputHandler.FullExpression);
        }

        [Theory]
        [InlineData("100+10", '+', 10, 110, "100 + 10% =")]
        [InlineData("100+10", '+', -10, 90, "100 + (-10%) =")]
        [InlineData("(-100)+10", '+', 10, -110, "(-100) + 10% =")]
        [InlineData("100*10", '*', 10, 10, "100 * 10% =")]
        [InlineData("100/10", '/', 10, 1000, "100 / 10% =")]
        public void HandleEquals_WithOperationPercent_CalculatesCorrectly(string expression, char op, double percent, double result, string expectedExpression)
        {
            // Arrange
            SetupPercentCalculation(percent, result, op);
            EnterExpression(expression);
            if (percent < 0) InputHandler.HandleToggleSign(); // Для від’ємного відсотка
            InputHandler.HandlePercent();

            // Act
            InputHandler.HandleEquals();

            // Assert
            Assert.Equal(result < 0 ? $"(-{Math.Abs(result)})" : result.ToString(), InputHandler.CurrentInput);
            Assert.Equal(expectedExpression, InputHandler.FullExpression);
        }

        [Fact]
        public void SimplePercent_Standalone_Test()
        {
            CalculatorMock.Setup(x => x.CalculateSimplePercent(20)).Returns(0.2);
            EnterDigits("20");
            InputHandler.HandlePercent();
            InputHandler.HandleEquals();
            Assert.Equal("0.2", InputHandler.CurrentInput);
        }

        [Fact]
        public void HandleEquals_WithPercentAfterResult_CalculatesSimplePercent()
        {
            // Arrange
            CalculatorMock.Setup(x => x.Calculate(It.IsAny<double>())).Returns(120); // Для будь-якого числа повертає 120
            CalculatorMock.Setup(x => x.CalculateSimplePercent(20)).Returns(0.2);

            // Симулюємо: 100 + 20 = 120
            EnterDigits("100");
            InputHandler.HandleOperator('+');
            EnterDigits("20");
            InputHandler.HandleEquals(); // Тут має викликатись Calculate(20)

            // Вводимо нове число для відсотка
            EnterDigits("20");
            InputHandler.HandlePercent();

            // Act
            InputHandler.HandleEquals();

            // Assert
            Assert.Equal("0.2", InputHandler.CurrentInput);
            CalculatorMock.Verify(x => x.Calculate(It.IsAny<double>()), Times.Once); // Перевіряємо, що Calculate викликався
            CalculatorMock.Verify(x => x.CalculateSimplePercent(20), Times.Once);
        }

        [Theory]
        [InlineData("50+10", "10%", "50 + ")]
        [InlineData("100+", "", "100 + ")]
        public void HandlePercent_AfterOperator_BehavesCorrectly(string expression, string expectedInput, string expectedExpression)
        {
            // Arrange
            EnterExpression(expression);

            // Act
            InputHandler.HandlePercent();

            // Assert
            Assert.Equal(expectedInput, InputHandler.CurrentInput);
            Assert.Equal(expectedExpression, InputHandler.FullExpression);
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
        public void HandlePercent_AfterErrorState_ResetsState()
        {
            // Arrange
            CalculatorMock.Setup(x => x.ErrorState).Returns(true);
            EnterDigits("5");

            // Act
            InputHandler.HandlePercent();

            // Assert
            Assert.Equal("5%", InputHandler.CurrentInput);
            Assert.False(InputHandler.ErrorState);
        }

        [Fact]
        public void HandlePercent_MultiplePercentSigns_OnlyFirstIsProcessed()
        {
            // Arrange
            EnterDigits("50");
            InputHandler.HandlePercent();

            // Act
            InputHandler.HandlePercent(); // Другий знак % ігнорується

            // Assert
            Assert.Equal("50%", InputHandler.CurrentInput);
        }

        [Fact]
        public void HandlePercentBetweenNumbers_WithSpaces_CalculatesCorrectly()
        {
            // Arrange
            CalculatorMock.Setup(x => x.CalculatePercentOfNumber(5, 100)).Returns(5);
            EnterDigits("5");
            InputHandler.HandlePercent();
            EnterDigits("100");

            // Act
            InputHandler.HandleEquals();

            // Assert
            Assert.Equal("5", InputHandler.CurrentInput);
            Assert.Equal("5%100 =", InputHandler.FullExpression);
        }

        [Fact]
        public void HandleEquals_WithNegativePercent_CalculatesCorrectly()
        {
            // Arrange
            SetupPercentCalculation(-3, -0.03);
            EnterDigits("3");
            InputHandler.HandlePercent();
            InputHandler.HandleToggleSign(); // Зміна знаку до чи після % дає той самий результат

            // Act
            InputHandler.HandleEquals();

            // Assert
            Assert.Equal("(-0.03)", InputHandler.CurrentInput);
            Assert.Equal("(-3%) =", InputHandler.FullExpression);
        }
    }
}