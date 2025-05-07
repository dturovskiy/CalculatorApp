using CalculatorCore.Services;
using System.Diagnostics;

namespace CalculatorCore
{
    public class CalculatorEngine : ICalculatorEngine
    {

        private double _storedNumber = 0;
        private char _pendingOperator = '\0';
        private bool _errorState = false;

        public double StoredNumber => _storedNumber;
        public char PendingOperator => _pendingOperator;
        public bool HasPendingOperation => _pendingOperator != '\0';
        public bool ErrorState => _errorState;

        public void SetOperation(double number, char operation)
        {
            if (!IsValidOperator(operation))
                throw new ArgumentException("Invalid operator. Allowed: +, -, *, /");

            _storedNumber = number;
            _pendingOperator = operation;
            _errorState = false;
        }

        public double Calculate(double secondNumber)
        {
            if (_errorState || !HasPendingOperation)
                throw new InvalidOperationException("No pending operation or in error state");

            try
            {
                double result = _pendingOperator switch
                {
                    '+' => _storedNumber + secondNumber,
                    '-' => _storedNumber - secondNumber,
                    '*' => _storedNumber * secondNumber,
                    '/' => secondNumber != 0 ? _storedNumber / secondNumber
                           : throw new DivideByZeroException("Division by zero is not allowed"),
                    _ => throw new InvalidOperationException("Unknown operator")
                };
                _errorState = false; // Скидаємо помилку при успішному обчисленні
                return result;
            }
            catch (Exception ex) when (ex is DivideByZeroException or InvalidOperationException)
            {
                _errorState = true; // Встановлюємо стан помилки
                throw;
            }
            finally
            {
                _pendingOperator = '\0';
            }
        }

        public double CalculateSimplePercent(double number) => number / 100;

        public double CalculatePercentOfNumber(double percent, double number)
        {
            // Логування значень перед обчисленням
            Debug.WriteLine($"Percent: {percent}, Number: {number}");

            double result = (percent / 100) * number;

            // Логування результату
            Debug.WriteLine($"Calculated Result: {result}");

            return result;
        }


        public double CalculateWithPercent(double percent)
        {
            if (!HasPendingOperation)
            {
                _errorState = true;
                throw new InvalidOperationException("No pending operation");
            }

            try
            {
                double percentValue = _pendingOperator switch
                {
                    '+' => _storedNumber * (1 + percent / 100),
                    '-' => _storedNumber * (1 - percent / 100),
                    '*' => _storedNumber * (percent / 100),
                    '/' => percent != 0 ? _storedNumber / (percent / 100)
                           : throw new DivideByZeroException("Division by zero"),
                    _ => throw new InvalidOperationException($"Unsupported operator '{_pendingOperator}' for percentage")
                };

                // Округлення до 12 знаків для уникнення проблем з плаваючою комою
                double roundedValue = Math.Round(percentValue, 12);

                // Якщо після округлення число ціле, повертаємо без дробової частини
                return roundedValue % 1 == 0 ? Math.Truncate(roundedValue) : roundedValue;
            }
            catch (Exception)
            {
                _errorState = true;
                throw;
            }
            finally
            {
                _pendingOperator = '\0'; // Скидаємо оператор після обчислення
            }
        }

        public void Reset()
        {
            _storedNumber = 0;
            _pendingOperator = '\0';
            _errorState = false;
        }

        private static bool IsValidOperator(char op) => op is '+' or '-' or '*' or '/';
    }
}