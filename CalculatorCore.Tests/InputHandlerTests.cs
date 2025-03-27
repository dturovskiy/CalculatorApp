using CalculatorCore.Services;
using Moq;
using System.Globalization;

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

        [Fact]
        public void Constructor_InitializesEmptyState()
        {
            Assert.Equal("", _handler.CurrentInput);
            Assert.Equal("", _handler.FullExpression);
            Assert.True(_handler.IsNewInput);
            Assert.False(_handler.ErrorState);
        }

        [Theory]
        [InlineData("5", "5")]
        [InlineData("0", "0")]
        public void HandleDigit_FirstDigit_SetsCurrentInput(string input, string expected)
        {
            _handler.HandleDigit(input);
            Assert.Equal(expected, _handler.CurrentInput);
            Assert.False(_handler.IsNewInput);
        }

        [Fact]
        public void HandleDigit_AfterErrorState_ResetsState()
        {
            // Симулюємо помилку
            _calculatorMock.Setup(x => x.ErrorState).Returns(true);
            _handler.HandleDigit("5");

            Assert.Equal("5", _handler.CurrentInput);
            Assert.False(_handler.ErrorState);
        }

        [Fact]
        public void HandleOperator_WithNegativeNumber_AddsParenthesesToExpression()
        {
            _handler.HandleDigit("5");
            _handler.HandleToggleSign();
            _handler.HandleOperator('+');

            Assert.Equal("(-5) + ", _handler.FullExpression);
        }

        [Fact]
        public void HandleDecimalPoint_FirstInput_StartsWithZeroPoint()
        {
            _handler.HandleDecimalPoint();
            Assert.Equal("0.", _handler.CurrentInput);
        }

        [Fact]
        public void HandleToggleSign_WithEmptyInput_DoesNothing()
        {
            _handler.HandleToggleSign();
            Assert.Equal("", _handler.CurrentInput);
        }

        [Fact]
        public void HandleEquals_WithDivisionByZero_SetsErrorState()
        {
            // Налаштовуємо мок для повернення помилки
            _calculatorMock.Setup(x => x.ErrorState).Returns(true);
            _calculatorMock.Setup(x => x.Calculate(It.IsAny<double>())).Returns(double.NaN);

            _handler.HandleDigit("5");
            _handler.HandleOperator('/');
            _handler.HandleDigit("0");
            _handler.HandleEquals();

            Assert.Equal("ERROR", _handler.CurrentInput);
            Assert.True(_handler.ErrorState);
        }

        [Fact]
        public void HandleClear_PartialClear_RemovesLastDigit()
        {
            _handler.HandleDigit("1");
            _handler.HandleDigit("2");
            _handler.HandleClear(false);

            Assert.Equal("1", _handler.CurrentInput);
        }

        [Fact]
        public void HandleEquals_WithCultureSpecificFormat_ParsesCorrectly()
        {
            // Симулюємо французьку культуру (використовує кому замість крапки)
            var originalCulture = Thread.CurrentThread.CurrentCulture;
            Thread.CurrentThread.CurrentCulture = new CultureInfo("fr-FR");

            try
            {
                _calculatorMock.Setup(x => x.Calculate(It.IsAny<double>())).Returns(8.5);

                _handler.HandleDigit("5");
                _handler.HandleDecimalPoint();
                _handler.HandleDigit("2");
                _handler.HandleOperator('+');
                _handler.HandleDigit("3");
                _handler.HandleDecimalPoint();
                _handler.HandleDigit("3");
                _handler.HandleEquals();

                // Перевіряємо, що вивід у InvariantCulture (з крапкою)
                Assert.Equal("8.5", _handler.CurrentInput);
            }
            finally
            {
                Thread.CurrentThread.CurrentCulture = originalCulture;
            }
        }

        [Fact]
        public void HandleOperator_AfterEquals_UsesResultAsFirstOperand()
        {
            // Налаштовуємо мок для повернення результату
            _calculatorMock.Setup(x => x.Calculate(It.IsAny<double>())).Returns(8);

            _handler.HandleDigit("5");
            _handler.HandleOperator('+');
            _handler.HandleDigit("3");
            _handler.HandleEquals();
            _handler.HandleOperator('-');

            Assert.Equal("8 - ", _handler.FullExpression);
        }

        [Fact]
        public void Reset_ClearsAllState()
        {
            _handler.HandleDigit("5");
            _handler.HandleOperator('+');
            _handler.Reset();

            Assert.Equal("", _handler.CurrentInput);
            Assert.Equal("", _handler.FullExpression);
            Assert.True(_handler.IsNewInput);
            Assert.False(_handler.ErrorState);
        }

        [Theory]
        [InlineData("5", "+", "3", "5 + 3 =")]
        [InlineData("5", "+", "-3", "5 + (-3) =")]
        [InlineData("-5", "+", "3", "(-5) + 3 =")]
        [InlineData("-5", "+", "-3", "(-5) + (-3) =")]
        [InlineData("5", "-", "-3", "5 - (-3) =")]
        [InlineData("5.2", "+", "-3.1", "5.2 + (-3.1) =")]
        public void Test_ExpressionFormatting(string first, string op, string second, string expected)
        {
            var handler = new InputHandler(new CalculatorEngine());

            // Обробка першого числа
            ProcessInputWithSign(handler, first);

            // Введення оператора
            handler.HandleOperator(op[0]);

            // Обробка другого числа
            ProcessInputWithSign(handler, second);

            // Обчислення
            handler.HandleEquals();

            Assert.Equal(expected, handler.FullExpression);
        }

        private void ProcessInputWithSign(IInputHandler handler, string input)
        {
            bool isNegative = input.StartsWith("-");
            string number = isNegative ? input.Substring(1) : input;

            // Введення цифр та крапки
            foreach (var c in number)
            {
                if (c == '.')
                    handler.HandleDecimalPoint();
                else
                    handler.HandleDigit(c.ToString());
            }

            // Додаємо мінус в кінці (як це робить UI)
            if (isNegative)
            {
                handler.HandleToggleSign();
            }
        }

        [Fact]
        public void HandleToggleSign_AddsParenthesesInHistory()
        {
            // Arrange
            var engine = new CalculatorEngine();
            var handler = new InputHandler(engine);

            // Act
            handler.HandleDigit("5");
            handler.HandleOperator('+');
            handler.HandleDigit("3");
            handler.HandleToggleSign(); // Змінюємо 3 на -3
            handler.HandleEquals();

            // Assert
            Assert.Equal("5 + (-3) =", handler.FullExpression);
        }

        [Fact]
        public void AfterEquals_DecimalOrSign_ClearsHistory()
        {
            var handler = new InputHandler(new CalculatorEngine());

            // 5 + 3 = 8
            handler.HandleDigit("5");
            handler.HandleOperator('+');
            handler.HandleDigit("3");
            handler.HandleEquals();

            // Натискаємо . після =
            handler.HandleDecimalPoint();
            Assert.Equal("0.", handler.CurrentInput);
            Assert.Equal("", handler.FullExpression); // Історія очищена!
        }
    }
}