using CalculatorCore.Services;
using System.Globalization;

namespace CalculatorCore
{
    public class InputHandler(ICalculatorEngine calculator, ExpressionFormatter formatter) : IInputHandler
    {
        private readonly ICalculatorEngine _calculator = calculator ?? throw new ArgumentNullException(nameof(calculator));
        private readonly ExpressionFormatter _formatter = formatter ?? throw new ArgumentNullException(nameof(formatter));

        private string _currentInput = "";
        private string _fullExpression = "";
        private bool _isNewInput = true;
        private bool _hasDecimalPoint = false;
        private bool _errorState = false;

        public string CurrentInput => _errorState ? "ERROR" : _currentInput;
        public string FullExpression => _fullExpression;
        public bool IsNewInput => _isNewInput;
        public bool ErrorState => _errorState;

        public void HandleDigit(string digit)
        {
            if (ValidateDigitInput(digit)) return;

            if (_isNewInput)
            {
                StartNewInput(digit);
            }
            else
            {
                AppendToCurrentInput(digit);
            }
        }

        public void HandleOperator(char op)
        {
            if (ValidateOperatorInput(op)) return; // Додаємо перевірку на початку

            // Решта коду залишається без змін
            if (_fullExpression.Contains('='))
            {
                HandleOperatorAfterEquals(op);
                return;
            }
            HandleRegularOperator(op);
        }

        private void HandleOperatorAfterEquals(char op)
        {
            // 1. Витягуємо та валідуємо результат
            string result = _currentInput;
            if (string.IsNullOrEmpty(result)) return;

            // 2. Форматуємо для історії (з урахуванням дужок)
            string formattedResult = _formatter.FormatForHistory(result);

            // 3. Створюємо новий вираз
            _fullExpression = $"{formattedResult} {op} ";

            _currentInput = "";
            _isNewInput = true;
            _hasDecimalPoint = false;
        }

        private void HandleRegularOperator(char op)
        {
            // 1. Перевірка на відсотки
            if (_fullExpression.Contains('%')) return;

            // 2. Якщо є поточний ввід
            if (!string.IsNullOrEmpty(_currentInput))
            {
                UpdateExpressionWithOperator(op);
                return;
            }

            // 3. Спроба використати попередній результат
            if (ShouldUseResultFromPreviousCalculation())
            {
                UseResultFromPreviousCalculation();
                UpdateExpressionWithOperator(op);
                return;
            }

            // 4. Зміна оператора
            if (CanChangeOperator())
            {
                ChangeOperator(op);
                return;
            }

            // 5. Стандартна поведінка
            _fullExpression = $"0 {op} ";
            _isNewInput = true;
        }

        public void HandleDecimalPoint()
        {
            if (_errorState) return;

            if (_isNewInput)
            {
                StartNewDecimalInput();
            }
            else if (!_hasDecimalPoint)
            {
                AddDecimalPointToInput();
            }
        }

        public void HandleToggleSign()
        {
            if (_errorState || string.IsNullOrEmpty(_currentInput)) return;

            if (_isNewInput && !string.IsNullOrEmpty(_fullExpression))
            {
                _fullExpression = "";
            }

            _currentInput = _formatter.ToggleSign(_currentInput);
        }

        public void HandleClear(bool fullClear)
        {
            if (fullClear || string.IsNullOrEmpty(_currentInput))
            {
                Reset();
            }
            else
            {
                Backspace();
            }
        }

        public void HandlePercent()
        {
            if (_errorState || string.IsNullOrEmpty(_currentInput)) return;
            if (_currentInput.Contains('%')) return;

            _currentInput += "%";
            _isNewInput = false;
        }

        public void HandleEquals()
        {
            if (_errorState || string.IsNullOrEmpty(_currentInput) || _fullExpression.Contains('='))
            {
                return; // Нічого не робимо, якщо вже є результат
            }

            try
            {
                ProcessCalculation();
                _isNewInput = true;
            }
            catch
            {
                SetErrorState();
            }
        }

        public void Reset()
        {
            _currentInput = "";
            _fullExpression = "";
            _isNewInput = true;
            _hasDecimalPoint = false;
            _errorState = false;
        }

        #region Private Helper Methods

        private bool ValidateDigitInput(string digit)
        {
            if (_errorState)
            {
                Reset();
                return true;
            }

            return digit.Length != 1 || !char.IsDigit(digit[0]);
        }

        private void StartNewInput(string digit)
        {
            _fullExpression = _fullExpression.Contains('=') ? "" : _fullExpression;
            _currentInput = digit == "0" ? "0" : digit;
            _isNewInput = false;
            _hasDecimalPoint = false;
        }

        private void AppendToCurrentInput(string digit)
        {
            if (_currentInput == "0" && digit != ".")
            {
                return;
            }
            _currentInput += digit;
        }

        private bool ValidateOperatorInput(char op)
        {
            if (_errorState)
            {
                Reset();
                return true;
            }

            return _fullExpression.Contains('%');
        }

        private bool ShouldUseResultFromPreviousCalculation()
        {
            return string.IsNullOrEmpty(_currentInput) &&
                  (_fullExpression.Contains('=') || !string.IsNullOrEmpty(_fullExpression));
        }

        private void UseResultFromPreviousCalculation()
        {
            if (_fullExpression.Contains('='))
            {
                string result = _fullExpression.Split('=')[1].Trim();
                if (result.StartsWith("(") && result.EndsWith(")"))
                {
                    result = result.Substring(1, result.Length - 2);
                }
                _currentInput = result;
            }
            else
            {
                _currentInput = _formatter.Parse(_fullExpression.Trim()).ToString(CultureInfo.InvariantCulture);
            }
            _fullExpression = "";
        }

        private void UpdateExpressionWithOperator(char op)
        {
            string displayNumber = _formatter.FormatForHistory(_currentInput);
            _fullExpression = $"{displayNumber} {op} "; // Наприклад: "5 + "
            _isNewInput = true;
            _hasDecimalPoint = false;
            _currentInput = "";
        }

        private bool CanChangeOperator()
        {
            return !string.IsNullOrEmpty(_fullExpression) && !_fullExpression.Contains('=');
        }

        private void ChangeOperator(char op)
        {
            _fullExpression = _fullExpression[..^2] + op + " ";
        }

        private void StartNewDecimalInput()
        {
            _currentInput = "0.";
            _fullExpression = "";
            _isNewInput = false;
            _hasDecimalPoint = true;
        }

        private void AddDecimalPointToInput()
        {
            _currentInput += ".";
            _hasDecimalPoint = true;
        }

        private void Backspace()
        {
            _currentInput = _currentInput[..^1];

            if (_currentInput == "0" || string.IsNullOrEmpty(_currentInput))
            {
                _isNewInput = true;
                _currentInput = "";
            }
        }

        private void ProcessCalculation()
        {
            ExpressionType type = DetermineExpressionType();

            switch (type)
            {
                case ExpressionType.PercentOperation:
                case ExpressionType.PercentOfNumber:
                case ExpressionType.StandalonePercent:
                    ProcessPercentCalculation();
                    break;
                case ExpressionType.RegularOperation:
                    ProcessRegularCalculation();
                    break;
                case ExpressionType.SingleNumber: // Нова обробка
                    ProcessSingleNumber();
                    break;
                default:
                    SetErrorState();
                    break;
            }
        }

        private void ProcessSingleNumber()
        {
            _fullExpression = _formatter.FormatExpression(
                leftPart: _currentInput,
                operation: "",
                rightPart: ""
            );
        }

        private void ProcessPercentCalculation()
        {
            switch (DetermineExpressionType())
            {
                case ExpressionType.StandalonePercent:
                    ProcessStandalonePercent();
                    break;
                case ExpressionType.PercentOperation:
                    ProcessPercentOperation();
                    break;
                case ExpressionType.PercentOfNumber:
                    ProcessPercentOfNumber();
                    break;
                default:
                    SetErrorState();
                    break;
            }
        }

        private void ProcessStandalonePercent()
        {
            try
            {
                string numberStr = _formatter.ExtractNumberFromPercent(_currentInput);
                double number = _formatter.Parse(numberStr);
                double result = _calculator.CalculateSimplePercent(number);

                _currentInput = _formatter.FormatResultWithParentheses(result);
                _fullExpression = _formatter.FormatExpression(
                    leftPart: _formatter.FormatPercentForHistory(number));
            }
            catch
            {
                SetErrorState();
            }
        }

        private void ProcessPercentOperation()
        {
            var opParts = _fullExpression.Split([' '], StringSplitOptions.RemoveEmptyEntries);
            if (opParts.Length < 2)
            {
                SetErrorState();
                return;
            }

            try
            {
                double baseNum = _formatter.Parse(opParts[0]);
                char operation = opParts[1][0];
                string percentStr = _formatter.ExtractNumberFromPercent(_currentInput);
                double percent = _formatter.Parse(percentStr);

                _calculator.SetOperation(baseNum, operation);
                double result = _calculator.CalculateWithPercent(percent);

                _currentInput = _formatter.FormatResultWithParentheses(result);
                _fullExpression = _formatter.FormatExpression(
                    leftPart: _formatter.FormatDisplay(baseNum),
                    operation: operation.ToString(),
                    rightPart: _formatter.FormatPercentForHistory(percent));
            }
            catch
            {
                SetErrorState();
            }
        }

        private void ProcessPercentOfNumber()
        {
            string[] parts = _currentInput.Split(['%'], StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 2)
            {
                SetErrorState();
                return;
            }

            try
            {
                double percentOf = _formatter.Parse(parts[0].Trim());
                double number = _formatter.Parse(parts[1].Trim());

                double result = _calculator.CalculatePercentOfNumber(percentOf, number);
                _currentInput = _formatter.FormatResultWithParentheses(result);
                _fullExpression = _formatter.FormatExpression(
                    leftPart: $"{percentOf}%",
                    rightPart: _formatter.FormatDisplay(number));
            }
            catch
            {
                SetErrorState();
            }
        }

        private void ProcessRegularCalculation()
        {
            if (string.IsNullOrEmpty(_fullExpression)/* || _fullExpression.Contains('=')*/) return;

            try
            {
                var parts = _fullExpression.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 2) throw new FormatException("Invalid expression format");

                double firstNumber = _formatter.Parse(parts[0]);
                char op = parts[1][0];
                double secondNumber = _formatter.Parse(_currentInput);

                _calculator.SetOperation(firstNumber, op);
                double result = _calculator.Calculate(secondNumber);

                _currentInput = _formatter.FormatResultWithParentheses(result);
                _fullExpression = _formatter.FormatExpression(
                    leftPart: parts[0],
                    operation: op.ToString(),
                    rightPart: _formatter.FormatSecondOperand(secondNumber));
            }
            catch
            {
                SetErrorState();
            }
        }

        private void SetErrorState()
        {
            _errorState = true;
        }

        private ExpressionType DetermineExpressionType()
        {
            bool hasOperator = _fullExpression.Contains(" + ") ||
                              _fullExpression.Contains(" - ") ||
                              _fullExpression.Contains(" * ") ||
                              _fullExpression.Contains(" / ");

            bool currentEndsWithPercent = _currentInput.EndsWith('%');
            bool currentHasPercent = _currentInput.Contains('%');
            bool historyHasPercent = _fullExpression.Contains('%');

            // 1. A% (Standalone)
            if (currentEndsWithPercent && !hasOperator && !historyHasPercent)
                return ExpressionType.StandalonePercent;

            // 2. A + B% (Operation with percent)
            if (currentEndsWithPercent && hasOperator)
                return ExpressionType.PercentOperation;

            // 3. A% B (Percent of number)
            if (currentHasPercent && !currentEndsWithPercent && !hasOperator)
                return ExpressionType.PercentOfNumber;

            // 4. Regular operation
            if (hasOperator)
                return ExpressionType.RegularOperation;

            // 5. Single number (new case)
            if (!string.IsNullOrEmpty(_currentInput) && !hasOperator)
                return ExpressionType.SingleNumber;

            return ExpressionType.Unknown;
        }
    }

    #endregion
}