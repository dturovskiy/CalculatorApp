using CalculatorCore.Services;

namespace CalculatorCore
{
    public class InputHandler : IInputHandler
    {
        private string _currentInput = "";
        private string _fullExpression = "";
        private bool _isNewInput = true;
        private bool _hasDecimalPoint = false;
        private bool _errorState = false;

        public string CurrentInput => _currentInput;
        public string FullExpression => _fullExpression;
        public bool IsNewInput => _isNewInput;
        public bool ErrorState => _errorState;

        public void HandleDigit(string digit)
        {
            if (_errorState) return;

            if (_isNewInput)
            {
                if (_fullExpression.Contains('='))
                {
                    _fullExpression = "";
                }

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
            if (_errorState) return;

            if (string.IsNullOrEmpty(_currentInput) && _fullExpression.Contains('='))
            {
                _currentInput = _fullExpression.Split('=')[1].Trim();
                _fullExpression = "";
            }

            if (!string.IsNullOrEmpty(_currentInput))
            {
                string displayNumber = _currentInput.StartsWith('-')
                    ? $"({_currentInput})"
                    : _currentInput;

                _fullExpression = $"{displayNumber} {op} ";
                _isNewInput = true;
                _currentInput = "";
            }
            else if (!string.IsNullOrEmpty(_fullExpression) && !_fullExpression.Contains('='))
            {
                _fullExpression = _fullExpression[..^2] + op + " ";
            }
        }

        public void HandleDecimalPoint()
        {
            if (_errorState) return;

            if (_isNewInput)
            {
                _currentInput = "0.";
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

        public void HandleEquals()
        {
            if (_errorState || string.IsNullOrEmpty(_fullExpression))
                return;

            if (_fullExpression.Contains('=') || !_fullExpression.Contains(' '))
                return;

            string currentNumber = string.IsNullOrEmpty(_currentInput)
                ? "0"
                : _currentInput;

            string displayNumber = currentNumber.StartsWith('-')
                ? $"({currentNumber})"
                : currentNumber;

            _fullExpression = $"{_fullExpression}{displayNumber} =";
            _isNewInput = true;
        }

        public void Reset()
        {
            _currentInput = "";
            _fullExpression = "";
            _isNewInput = true;
            _hasDecimalPoint = false;
            _errorState = false;
        }
    }
}