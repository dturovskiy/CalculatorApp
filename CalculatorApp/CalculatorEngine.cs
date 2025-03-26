namespace CalculatorApp
{
    public class CalculatorEngine
    {
        private double _storedNumber = 0;
        private char _pendingOperator = '\0'; // '\0' — ознака відсутності оператора
        private bool _errorState = false;

        // Публічні властивості для доступу до стану
        public double StoredNumber
        {
            get
            {
                return _storedNumber;
            }

            set
            {
                _storedNumber = value;
            }
        }

        public char PendingOperator => _pendingOperator;
        public bool ErrorState => _errorState;

        /// <summary>
        /// Зберігає число та оператор для майбутнього обчислення.
        /// </summary>
        public void SetOperation(double number, char operation)
        {
            if (!IsValidOperator(operation))
                throw new ArgumentException("Невірний оператор");

            _storedNumber = number;
            _pendingOperator = operation;
            _errorState = false;
        }

        /// <summary>
        /// Виконує обчислення між збереженим числом і другим числом.
        /// </summary>
        public double Calculate(double secondNumber)
        {
            if (_errorState || _pendingOperator == '\0')
                return 0;

            try
            {
                return _pendingOperator switch
                {
                    '+' => _storedNumber + secondNumber,
                    '-' => _storedNumber - secondNumber,
                    '*' => _storedNumber * secondNumber,
                    '/' => secondNumber != 0 ? _storedNumber / secondNumber
                           : throw new DivideByZeroException(),
                    _ => throw new InvalidOperationException("Невідомий оператор")
                };
            }
            catch (Exception ex) when (ex is DivideByZeroException or InvalidOperationException)
            {
                _errorState = true;
                return 0;
            }
            finally
            {
                _pendingOperator = '\0'; // Скидаємо оператор після обчислення
            }
        }

        /// <summary>
        /// Скидає стан калькулятора.
        /// </summary>
        public void Reset()
        {
            _storedNumber = 0;
            _pendingOperator = '\0';
            _errorState = false;
        }

        // Перевіряє, чи оператор є допустимим
        private static bool IsValidOperator(char op)
        {
            return op is '+' or '-' or '*' or '/';
        }
    }
}