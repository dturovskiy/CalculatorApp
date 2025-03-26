using CalculatorApp.Services;

namespace CalculatorApp
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
                // Якщо введення починається після "=" (тобто історія завершена), очистити її
                if (_fullExpression.Contains("="))
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
                    return;

                _currentInput += digit;
            }
        }

        public void HandleOperator(char op)
        {
            if (_errorState) return;

            if (!string.IsNullOrEmpty(_currentInput))
            {
                // Тут буде логіка обробки оператора (пізніше додамо CalculatorEngine)
                _fullExpression = $"{_currentInput} {op} ";
                _isNewInput = true;
                _currentInput = "";
                _hasDecimalPoint = false;
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
                ? _currentInput.Substring(1)
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
                _currentInput = _currentInput.Length > 1
                    ? _currentInput[..^1]
                    : "0";
            }
        }

        public void HandleEquals()
        {
            if (_errorState) return;

            // Тут буде логіка обчислення (пізніше додамо CalculatorEngine)
            _fullExpression = $"{_fullExpression} {_currentInput} =";
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