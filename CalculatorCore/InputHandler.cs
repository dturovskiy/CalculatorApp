using CalculatorCore.Services;
using System.Globalization;

namespace CalculatorCore
{
    public class InputHandler(ICalculatorEngine calculator) : IInputHandler
    {
        private string _currentInput = "";
        private string _fullExpression = "";
        private bool _isNewInput = true;
        private bool _hasDecimalPoint = false;
        private bool _errorState = false;

        private readonly ICalculatorEngine _calculator = calculator;

        public string CurrentInput => _currentInput;
        public string FullExpression => _fullExpression;
        public bool IsNewInput => _isNewInput;
        public bool ErrorState => _errorState;

        public void HandleDigit(string digit)
        {
            if (_errorState)
            {
                Reset();
                return;
            }

            if (digit.Length != 1 || !char.IsDigit(digit[0])) return;

            if (_isNewInput)
            {
                // Завжди починаємо нове введення, незалежно від _fullExpression
                _fullExpression = _fullExpression.Contains('=') ? "" : _fullExpression;
                _currentInput = digit == "0" ? "0" : digit;
                _isNewInput = false;
                _hasDecimalPoint = false;
            }
            else
            {
                if (_currentInput == "0" && digit != ".")
                {
                    return;
                }
                _currentInput += digit;
            }
        }

        public void HandleOperator(char op)
        {
            if (_errorState)
            {
                Reset();
                return;
            }

            // Якщо вже є вираз з % (A% B), ігноруємо оператор
            if (_fullExpression.Contains('%')) return;

            if (string.IsNullOrEmpty(_currentInput) && _fullExpression.Contains('='))
            {
                _currentInput = _fullExpression.Split('=')[1].Trim();
                _fullExpression = "";
            }

            if (!string.IsNullOrEmpty(_currentInput))
            {
                // Форматуємо число для історії
                string displayNumber = FormatForHistory(_currentInput);
                _fullExpression = $"{displayNumber} {op} ";
                _isNewInput = true;
                _currentInput = "";
            }
            else if (!string.IsNullOrEmpty(_fullExpression) && !_fullExpression.Contains('='))
            {
                // Зміна оператора
                _fullExpression = _fullExpression[..^2] + op + " ";
            }
        }

        public void HandleDecimalPoint()
        {
            if (_errorState)
            {
                return;
            }

            if (_isNewInput)
            {
                _currentInput = "0.";
                _fullExpression = "";
                _isNewInput = false;
                _hasDecimalPoint = true;
            }
            else if (!_hasDecimalPoint)
            {
                _currentInput += ".";
                _hasDecimalPoint = true;
            }
        }

        public void HandleToggleSign()
        {
            if (_errorState || string.IsNullOrEmpty(_currentInput)) return;

            if (_isNewInput && !string.IsNullOrEmpty(_fullExpression))
            {
                _fullExpression = "";
            }

            _currentInput = _currentInput.StartsWith('-')
                ? _currentInput[1..]
                : $"-{_currentInput}";
        }

        public void HandleClear(bool fullClear)
        {
            if (fullClear || string.IsNullOrEmpty(_currentInput))
            {
                Reset();
            }
            else
            {
                _currentInput = _currentInput[..^1];

                if (_currentInput == "0" || string.IsNullOrEmpty(_currentInput))
                {
                    _isNewInput = true;
                    _currentInput = "";
                }
            }
        }

        public void HandlePercent()
        {
            if (_errorState || string.IsNullOrEmpty(_currentInput)) return;
            if (_currentInput.Contains('%')) return;

            // Просто додаємо % до поточного введення
            _currentInput += "%";
            _isNewInput = false;
        }

        public void HandleEquals()
        {
            if (_errorState || string.IsNullOrEmpty(_currentInput)) return;

            try
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
                    default:
                        _errorState = true;
                        _currentInput = "ERROR";
                        break;
                }
            }
            catch
            {
                _errorState = true;
                _currentInput = "ERROR";
            }
            finally
            {
                _isNewInput = true;
            }
        }

        private void ProcessPercentCalculation()
        {
            switch (DetermineExpressionType())
            {
                case ExpressionType.StandalonePercent:
                    double simplePercent = ParseNumber(_currentInput.TrimEnd('%'));
                    _currentInput = FormatNumber(_calculator.CalculateSimplePercent(simplePercent));
                    _fullExpression = $"{FormatNumber(simplePercent)}% =";
                    break;

                case ExpressionType.PercentOperation:
                    var opParts = _fullExpression.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (opParts.Length < 2)
                    {
                        _errorState = true;
                        _currentInput = "ERROR";
                        break;
                    }

                    double baseNum = ParseNumber(opParts[0]);
                    char operation = opParts[1][0];
                    double percent = ParseNumber(_currentInput.TrimEnd('%'));

                    _calculator.SetOperation(baseNum, operation); // Фікс: передаємо baseNum!
                    double result = _calculator.CalculateWithPercent(percent);

                    _currentInput = FormatNumber(result);
                    _fullExpression = $"{FormatForHistory(FormatNumber(baseNum))} {operation} {percent}% =";
                    break;

                case ExpressionType.PercentOfNumber:
                    // Розділяємо введений рядок на частини (A% B)
                    string[] parts = _currentInput.Split(new[] { '%' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length != 2)
                    {
                        _errorState = true;
                        _currentInput = "ERROR";
                        break;
                    }

                    string percentPart = parts[0].Trim();
                    string numberPart = parts[1].Trim();

                    if (!double.TryParse(percentPart, NumberStyles.Any, CultureInfo.InvariantCulture, out double percentOf) ||
                        !double.TryParse(numberPart, NumberStyles.Any, CultureInfo.InvariantCulture, out double number))
                    {
                        _errorState = true;
                        _currentInput = "ERROR";
                        break;
                    }

                    double calculated = _calculator.CalculatePercentOfNumber(percentOf, number);
                    _currentInput = FormatNumber(calculated);
                    _fullExpression = $"{percentPart}%{FormatNumber(number)} =";
                    break;

                default:
                    _errorState = true;
                    _currentInput = "ERROR";
                    break;
            }
        }

        private void ProcessRegularCalculation()
        {
            if (string.IsNullOrEmpty(_fullExpression) || _fullExpression.Contains('=')) return;

            var parts = _fullExpression.Split(' ');
            double firstNumber = ParseNumber(parts[0]);
            char op = parts[1][0];
            double secondNumber = ParseNumber(_currentInput);

            _calculator.SetOperation(firstNumber, op);
            double result = _calculator.Calculate(secondNumber);

            if (_calculator.ErrorState)
            {
                _errorState = true;
                _currentInput = "ERROR";
            }
            else
            {
                _currentInput = FormatNumber(result);
                _fullExpression = $"{FormatForHistory(parts[0])} {op} {FormatSecondNumber(secondNumber)} =";
            }
        }


        private string FormatSecondNumber(double number)
        {
            return number < 0
                ? $"({FormatNumber(number)})" // Змінено тут
                 : FormatNumber(number); // І тут
        }

        public void Reset()
        {
            _currentInput = "";
            _fullExpression = "";
            _isNewInput = true;
            _hasDecimalPoint = false;
            _errorState = false;
        }

        // Допоміжні методи
        private static string FormatForHistory(string numberStr)
        {
            if (string.IsNullOrEmpty(numberStr))
                return "0";

            // Якщо число вже має дужки, повертаємо як є
            if (numberStr.StartsWith('(') && numberStr.EndsWith(')'))
                return numberStr;

            // Якщо число від'ємне, додаємо дужки
            if (numberStr.StartsWith('-'))
                return $"({numberStr})";

            return numberStr;
        }

        private static double ParseNumber(string numberStr)
        {
            if (string.IsNullOrEmpty(numberStr))
                return 0;

            // Видаляємо дужки якщо вони є
            string cleanNumber = numberStr.Replace("(", "").Replace(")", "");
            return double.Parse(cleanNumber, CultureInfo.InvariantCulture);
        }

        private static string FormatNumber(double number)
        {
            // Форматуємо з InvariantCulture і обрізаємо зайві нулі після крапки
            string formatted = number.ToString("0.###############", CultureInfo.InvariantCulture);
            return formatted;
        }

        private ExpressionType DetermineExpressionType()
        {
            bool hasOperator = _fullExpression.Contains(" + ") ||
                              _fullExpression.Contains(" - ") ||
                              _fullExpression.Contains(" * ") ||
                              _fullExpression.Contains(" / ");

            bool currentEndsWithPercent = _currentInput.EndsWith("%");
            bool currentHasPercent = _currentInput.Contains("%");
            bool historyHasPercent = _fullExpression.Contains("%");
            bool historyEndsWithPercent = _fullExpression.TrimEnd().EndsWith("%");

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

            return ExpressionType.Unknown;
        }
    }
}