using CalculatorCore.Services;
using Moq;

namespace CalculatorCore.Tests
{
    public class InputHandlerTests
    {
        private readonly Mock<ICalculatorEngine> _calculatorMock;
        private readonly InputHandler _handler;

        public InputHandlerTests()
        {
            _calculatorMock = new Mock<ICalculatorEngine>();
            _handler = new InputHandler(_calculatorMock.Object);
        }

        // ... (усі існуючі тести залишаються без змін) ...

        [Fact]
        public void HandlePercent_AddsPercentSymbol()
        {
            _handler.HandleDigit("5");
            _handler.HandleDigit("0");
            _handler.HandlePercent();

            Assert.Equal("50%", _handler.CurrentInput);
        }

        [Fact]
        public void HandleEquals_WithSimplePercent_CalculatesCorrectly()
        {
            _calculatorMock.Setup(x => x.CalculateSimplePercent(50)).Returns(0.5);

            _handler.HandleDigit("5");
            _handler.HandleDigit("0");
            _handler.HandlePercent();
            _handler.HandleEquals();

            Assert.Equal("0.5", _handler.CurrentInput);
            Assert.Equal("50% =", _handler.FullExpression);
        }

        [Fact]
        public void HandleEquals_WithOperationPercent_CalculatesCorrectly()
        {
            _calculatorMock.Setup(x => x.CalculateWithPercent(10)).Returns(110);

            _handler.HandleDigit("1");
            _handler.HandleDigit("0");
            _handler.HandleDigit("0");
            _handler.HandleOperator('+');
            _handler.HandleDigit("1");
            _handler.HandleDigit("0");
            _handler.HandlePercent();
            _handler.HandleEquals();

            Assert.Equal("110", _handler.CurrentInput);
            Assert.Equal("100 + 10% =", _handler.FullExpression);
        }

        [Fact]
        public void HandleEquals_WithPercentAfterResult_CalculatesSimplePercent()
        {
            // Перший вираз: 100 + 20 = 120
            _calculatorMock.Setup(x => x.Calculate(20)).Returns(120);

            _handler.HandleDigit("1");
            _handler.HandleDigit("0");
            _handler.HandleDigit("0");
            _handler.HandleOperator('+');
            _handler.HandleDigit("2");
            _handler.HandleDigit("0");
            _handler.HandleEquals();

            // Другий вираз: просто 20% (не від результату)
            _calculatorMock.Setup(x => x.CalculateSimplePercent(20)).Returns(0.2);

            _handler.HandleDigit("2");
            _handler.HandleDigit("0");
            _handler.HandlePercent();
            _handler.HandleEquals();

            Assert.Equal("0.2", _handler.CurrentInput);
            Assert.Equal("20% =", _handler.FullExpression);
        }

        [Fact]
        public void HandlePercent_AfterOperator_DoesNotAffectExpression()
        {
            _handler.HandleDigit("5");
            _handler.HandleDigit("0");
            _handler.HandleOperator('+');
            _handler.HandleDigit("1");
            _handler.HandleDigit("0");
            _handler.HandlePercent();

            Assert.Equal("10%", _handler.CurrentInput);
            Assert.Equal("50 + ", _handler.FullExpression);
        }

        [Fact]
        public void HandlePercent_OnEmptyInput_DoesNothing()
        {
            _handler.HandlePercent();
            Assert.Equal("", _handler.CurrentInput);
        }

        [Fact]
        public void HandlePercent_AfterErrorState_ResetsState()
        {
            _calculatorMock.Setup(x => x.ErrorState).Returns(true);
            _handler.HandleDigit("5");
            _handler.HandlePercent();

            Assert.Equal("5%", _handler.CurrentInput);
            Assert.False(_handler.ErrorState);
        }

        [Fact]
        public void HandleEquals_WithSimpleDecimalPercent_CalculatesCorrectly()
        {
            _calculatorMock.Setup(x => x.CalculateSimplePercent(12.5)).Returns(0.125);

            _handler.HandleDigit("1");
            _handler.HandleDigit("2");
            _handler.HandleDecimalPoint();
            _handler.HandleDigit("5");
            _handler.HandlePercent();
            _handler.HandleEquals();

            Assert.Equal("0.125", _handler.CurrentInput);
            Assert.Equal("12.5% =", _handler.FullExpression);
        }

        [Fact]
        public void HandleEquals_WithNegativePercent_CalculatesCorrectly()
        {
            _calculatorMock.Setup(x => x.SetOperation(100, '+'));
            _calculatorMock.Setup(x => x.CalculateWithPercent(-10)).Returns(90);

            // Правильна послідовність:
            _handler.HandleDigit("1");
            _handler.HandleDigit("0");
            _handler.HandleDigit("0");
            _handler.HandleOperator('+');
            _handler.HandleDigit("1");
            _handler.HandleDigit("0");
            _handler.HandleToggleSign(); // Мінус після введення 10
            _handler.HandlePercent();
            _handler.HandleEquals();

            Assert.Equal("90", _handler.CurrentInput);
            Assert.Equal("100 + -10% =", _handler.FullExpression);
        }

        [Fact]
        public void HandleEquals_WithNegativeBaseNumber_CalculatesCorrectly()
        {
            _calculatorMock.Setup(x => x.CalculateWithPercent(10)).Returns(-110);

            _handler.HandleDigit("1");
            _handler.HandleDigit("0");
            _handler.HandleDigit("0");
            _handler.HandleToggleSign(); // робимо від'ємним
            _handler.HandleOperator('+');
            _handler.HandleDigit("1");
            _handler.HandleDigit("0");
            _handler.HandlePercent();
            _handler.HandleEquals();

            Assert.Equal("-110", _handler.CurrentInput);
            Assert.Equal("(-100) + 10% =", _handler.FullExpression);
        }

        [Fact]
        public void HandleEquals_WithMultiplyPercent_CalculatesCorrectly()
        {
            _calculatorMock.Setup(x => x.CalculateWithPercent(10)).Returns(10);

            _handler.HandleDigit("1");
            _handler.HandleDigit("0");
            _handler.HandleDigit("0");
            _handler.HandleOperator('*');
            _handler.HandleDigit("1");
            _handler.HandleDigit("0");
            _handler.HandlePercent();
            _handler.HandleEquals();

            Assert.Equal("10", _handler.CurrentInput);
            Assert.Equal("100 * 10% =", _handler.FullExpression);
        }

        [Fact]
        public void HandleEquals_WithDividePercent_CalculatesCorrectly()
        {
            _calculatorMock.Setup(x => x.CalculateWithPercent(10)).Returns(1000);

            _handler.HandleDigit("1");
            _handler.HandleDigit("0");
            _handler.HandleDigit("0");
            _handler.HandleOperator('/');
            _handler.HandleDigit("1");
            _handler.HandleDigit("0");
            _handler.HandlePercent();
            _handler.HandleEquals();

            Assert.Equal("1000", _handler.CurrentInput);
            Assert.Equal("100 / 10% =", _handler.FullExpression);
        }

        [Fact]
        public void HandlePercent_AfterOperatorWithoutNumber_DoesNothing()
        {
            _handler.HandleDigit("1");
            _handler.HandleDigit("0");
            _handler.HandleDigit("0");
            _handler.HandleOperator('+');
            _handler.HandlePercent();

            Assert.Equal("", _handler.CurrentInput);
            Assert.Equal("100 + ", _handler.FullExpression);
        }

        [Fact]
        public void HandlePercent_MultiplePercentSigns_OnlyFirstIsProcessed()
        {
            _handler.HandleDigit("5");
            _handler.HandleDigit("0");
            _handler.HandlePercent();
            _handler.HandlePercent(); // другий знак % ігнорується

            Assert.Equal("50%", _handler.CurrentInput);
        }

        [Fact]
        public void FormatNumber_AlwaysUsesInvariantCulture()
        {
            // Налаштовуємо моки для всіх сценаріїв
            _calculatorMock.Setup(x => x.SetOperation(100, '+'));
            _calculatorMock.Setup(x => x.Calculate(25)).Returns(125);

            _calculatorMock.Setup(x => x.SetOperation(10.5, '*'));
            _calculatorMock.Setup(x => x.Calculate(2)).Returns(21);

            _calculatorMock.Setup(x => x.SetOperation(-5, '+'));
            _calculatorMock.Setup(x => x.Calculate(10.5)).Returns(5.5);

            // Сценарій 1: 100 + 25 = 125
            _handler.HandleDigit("1");
            _handler.HandleDigit("0");
            _handler.HandleDigit("0");
            _handler.HandleOperator('+');
            _handler.HandleDigit("2");
            _handler.HandleDigit("5");
            _handler.HandleEquals();
            Assert.Equal("125", _handler.CurrentInput);
            Assert.Equal("100 + 25 =", _handler.FullExpression);

            // Сценарій 2: 10.5 × 2 = 21
            _handler.HandleDigit("1");
            _handler.HandleDigit("0");
            _handler.HandleDecimalPoint();
            _handler.HandleDigit("5");
            _handler.HandleOperator('*');
            _handler.HandleDigit("2");
            _handler.HandleEquals();
            Assert.Equal("21", _handler.CurrentInput);
            Assert.Equal("10.5 * 2 =", _handler.FullExpression);

            // Сценарій 3: -5 + 10.5 = 5.5
            _handler.HandleDigit("5");
            _handler.HandleToggleSign(); // Мінус після введення числа!
            _handler.HandleOperator('+');
            _handler.HandleDigit("1");
            _handler.HandleDigit("0");
            _handler.HandleDecimalPoint();
            _handler.HandleDigit("5");
            _handler.HandleEquals();
            Assert.Equal("5.5", _handler.CurrentInput);
            Assert.Equal("(-5) + 10.5 =", _handler.FullExpression);
        }

        [Fact]
        public void HandlePercentBetweenNumbers_WithSpaces_CalculatesCorrectly()
        {
            // Arrange
            _calculatorMock.Setup(x => x.CalculatePercentOfNumber(5, 100)).Returns(5);

            // Act
            _handler.HandleDigit("5");
            _handler.HandlePercent();  // "5 % " у FullExpression
            _handler.HandleDigit("1");
            _handler.HandleDigit("0");
            _handler.HandleDigit("0");
            _handler.HandleEquals();

            // Assert
            Assert.Equal("5", _handler.CurrentInput);
            Assert.Equal("5%100 =", _handler.FullExpression);
        }
    }
}