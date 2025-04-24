using CalculatorCore.Services;
using Moq;
using System.Text;

namespace CalculatorCore.Tests
{
    public abstract class CalculatorTestBase : IDisposable
    {
        protected readonly Mock<ICalculatorEngine> CalculatorMock;
        protected readonly Mock<ExpressionFormatter> FormatterMock;
        protected readonly InputHandler InputHandler;
        protected readonly CalculatorEngine CalculatorEngine;

        protected CalculatorTestBase(bool useRealEngine = true)
        {
            CalculatorMock = new Mock<ICalculatorEngine>();
            FormatterMock = new Mock<ExpressionFormatter>();

            CalculatorEngine = new CalculatorEngine();
            InputHandler = useRealEngine
                ? new InputHandler(CalculatorEngine, new ExpressionFormatter())
                : new InputHandler(CalculatorMock.Object, FormatterMock.Object);
        }

        protected void EnterDigits(string digits)
        {
            foreach (var c in digits)
            {
                if (c == '.') InputHandler.HandleDecimalPoint();
                else if (c == '-') InputHandler.HandleToggleSign();
                else InputHandler.HandleDigit(c.ToString());
            }
        }

        protected void EnterExpression(string expression)
        {
            bool isNegative = false;
            int startIndex = 0;

            // Обробка випадку "(-100)"
            if (expression.StartsWith("(-"))
            {
                isNegative = true;
                startIndex = 2; // Пропускаємо "(-"
            }
            // Обробка звичайного від'ємного числа "-100"
            else if (expression.StartsWith("-"))
            {
                isNegative = true;
                startIndex = 1; // Пропускаємо "-"
            }

            // Вводимо число
            StringBuilder currentNumber = new StringBuilder();
            for (int i = startIndex; i < expression.Length; i++)
            {
                char c = expression[i];

                if (char.IsDigit(c) || c == '.')
                {
                    currentNumber.Append(c);
                }
                else if (c == ')')
                {
                    continue; // Пропускаємо закриваючу дужку
                }
                else
                {
                    // Якщо є число, яке треба ввести
                    if (currentNumber.Length > 0)
                    {
                        EnterDigits(currentNumber.ToString());
                        if (isNegative)
                        {
                            InputHandler.HandleToggleSign(); // Додаємо мінус після введення числа
                            isNegative = false;
                        }
                        currentNumber.Clear();
                    }

                    // Обробка оператора
                    if ("+-*/".Contains(c))
                    {
                        InputHandler.HandleOperator(c);
                    }
                }
            }

            // Вводимо останнє число (якщо залишилося)
            if (currentNumber.Length > 0)
            {
                EnterDigits(currentNumber.ToString());
                if (isNegative)
                {
                    InputHandler.HandleToggleSign();
                }
            }
        }


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
        }
    }
}