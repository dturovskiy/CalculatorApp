using CalculatorCore.Services;
using Moq;

namespace CalculatorCore.Tests
{
    /// <summary>
    /// Базовий клас для всіх тестів калькулятора.
    /// Містить спільну логіку ініціалізації та допоміжні методи.
    /// </summary>
    public abstract class CalculatorTestBase : IDisposable
    {
        // Спільні залежності для всіх тестів
        protected readonly Mock<ICalculatorEngine> CalculatorMock;
        protected readonly Mock<ExpressionFormatter> FormatterMock;
        protected readonly InputHandler InputHandler;
        protected readonly CalculatorEngine CalculatorEngine;

        protected CalculatorTestBase(bool useRealEngine = false)
        {
            CalculatorMock = new Mock<ICalculatorEngine>();
            FormatterMock = new Mock<ExpressionFormatter>();

            CalculatorEngine = new CalculatorEngine();
            InputHandler = useRealEngine
                ? new InputHandler(CalculatorEngine, new ExpressionFormatter())
                : new InputHandler(CalculatorMock.Object, FormatterMock.Object);
        }

        /// <summary>
        /// Вводить послідовність цифр у калькулятор
        /// </summary>
        protected void EnterDigits(string digits)
        {
            foreach (var c in digits)
            {
                if (c == '.') InputHandler.HandleDecimalPoint();
                else if (c == '-') InputHandler.HandleToggleSign();
                else InputHandler.HandleDigit(c.ToString());
            }
        }

        /// <summary>
        /// Вводить математичний вираз (цифри + оператори)
        /// </summary>
        protected void EnterExpression(string expression)
        {
            foreach (var c in expression)
            {
                if (char.IsDigit(c))
                    InputHandler.HandleDigit(c.ToString());
                else if ("+-*/".Contains(c))
                    InputHandler.HandleOperator(c);
                else if (c == '.')
                    InputHandler.HandleDecimalPoint();
                else if (c == ' ')
                {
                    // Явно обробляємо пробіли
                    // Вони важливі для форматування виразу
                }
            }
        }

        /// <summary>
        /// Налаштовує мок для операції з відсотками
        /// </summary>
        protected void SetupPercentCalculation(double input, double result, char? op = null)
        {
            if (op.HasValue)
            {
                CalculatorMock.Setup(x => x.CalculateWithPercent(input)).Returns(result);
                CalculatorMock.Setup(x => x.SetOperation(It.IsAny<double>(), op.Value));
            }
            else
            {
                CalculatorMock.Setup(x => x.CalculateSimplePercent(input)).Returns(result);
            }
        }

        public virtual void Dispose()
        {
            // Очищення ресурсів при необхідності
        }
    }
}