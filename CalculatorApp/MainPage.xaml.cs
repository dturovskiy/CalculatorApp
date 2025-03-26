using System.Globalization;

namespace CalculatorApp
{
    public partial class MainPage : ContentPage
    {
        private string _currentInput = "";
        private string _fullExpression = "";
        private bool _isNewInput = true;
        private bool _errorState = false;
        private bool _hasDecimalPoint = false;
        private readonly CalculatorEngine _calculator = new();

        public MainPage()
        {
            InitializeComponent();
            UpdateDisplay();
        }

        private void OnDigitClicked(object sender, EventArgs e)
        {
            if (_errorState) return;

            var button = (Button)sender;
            var digit = button.Text;

            if (_isNewInput)
            {
                // Очищаємо історію ТІЛЬКИ якщо це не продовження виразу
                if (_calculator.PendingOperator == '\0' && !string.IsNullOrEmpty(_fullExpression))
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

            UpdateDisplay();
        }

        private void OnToggleSignClicked(object sender, EventArgs e)
        {
            if (_calculator.ErrorState || string.IsNullOrEmpty(_currentInput) || _currentInput == "0")
                return;

            // Інший код залишається без змін...
            if (_calculator.PendingOperator == '\0' && !string.IsNullOrEmpty(_fullExpression))
            {
                _fullExpression = "";
            }

            switch (_currentInput)
            {
                case ".": _currentInput = "-0."; break;
                case "-.": _currentInput = "0."; break;
                case "-": _currentInput = "-0"; break;
                default:
                    _currentInput = _currentInput.StartsWith('-')
                        ? _currentInput.Substring(1)
                        : "-" + _currentInput;
                    break;
            }

            if (_calculator.PendingOperator == '\0')
            {
                if (double.TryParse(_currentInput, NumberStyles.Any, CultureInfo.InvariantCulture, out double number))
                {
                    _calculator.StoredNumber = number;
                }
                else
                {
                    _currentInput = "0";
                }
            }

            UpdateDisplay();
        }

        private void OnOperatorClicked(object sender, EventArgs e)
        {
            if (_calculator.ErrorState) return;

            var button = (Button)sender;
            char newOperator = button.Text[0];

            if (!string.IsNullOrEmpty(_currentInput))
            {
                if (_calculator.PendingOperator != '\0')
                {
                    OnEqualsClicked(sender, e);
                }

                // Додаємо дужки для від'ємних чисел
                string displayNumber = _currentInput;
                if (_currentInput.StartsWith('-'))
                {
                    displayNumber = $"({_currentInput})";
                }

                double number = double.Parse(_currentInput, CultureInfo.InvariantCulture);
                _calculator.SetOperation(number, newOperator);
                _fullExpression = $"{displayNumber} {newOperator} ";
            }
            else if (_calculator.PendingOperator != '\0')
            {
                // Зміна оператора без нового числа
                string displayNumber = _calculator.StoredNumber < 0
                    ? $"({_calculator.StoredNumber})"
                    : _calculator.StoredNumber.ToString(CultureInfo.InvariantCulture);

                _calculator.SetOperation(_calculator.StoredNumber, newOperator);
                _fullExpression = $"{displayNumber} {newOperator} ";
            }

            _isNewInput = true;
            _currentInput = "";
            _hasDecimalPoint = false;
            UpdateDisplay();
        }

        private void OnACClicked(object sender, EventArgs e)
        {
            if (_errorState) return;

            if (string.IsNullOrEmpty(_currentInput) || _currentInput == "0" || _isNewInput)
            {
                _currentInput = "";
                _fullExpression = "";
                _isNewInput = true;
                _errorState = false;
                _hasDecimalPoint = false;
            }
            else
            {
                _currentInput = _currentInput[0..^1];
                if (string.IsNullOrEmpty(_currentInput))
                {
                    _currentInput = "0";
                }
            }

            UpdateDisplay();
        }

        private void OnDecimalPointClicked(object sender, EventArgs e)
        {
            if (_errorState) return;

            // Якщо це результат попередньої операції (нова вхідна операція)
            if (_isNewInput)
            {
                _currentInput = "0.";
                _isNewInput = false;
                _hasDecimalPoint = true;
                _fullExpression = ""; // Очищаємо історію
            }
            else if (!_hasDecimalPoint)
            {
                _currentInput += ".";
                _hasDecimalPoint = true;
            }

            UpdateDisplay();
        }

        private void UpdateDisplay()
        {
            ACButton.Text = (string.IsNullOrEmpty(_currentInput) || _errorState || _isNewInput || _currentInput == "0"
                ? "AC"
                : "←");

            DisplayLabel.Text = FormatDisplayNumber(_currentInput);
            HistoryLabel.Text = _fullExpression;
        }

        private string FormatDisplayNumber(string input)
        {
            if (string.IsNullOrEmpty(input)) return "0";
            if (input == ".") return "0.";
            if (input == "-.") return "-0.";
            if (input.StartsWith(".")) return "0" + input;
            return input;
        }

        private void OnPercentClicked(object sender, EventArgs e)
        {
            DisplayAlert("Інформація", "Функція відсотків тимчасово недоступна", "OK");
        }

        private void OnEqualsClicked(object sender, EventArgs e)
        {
            if (_calculator.ErrorState || _calculator.PendingOperator == '\0')
                return;

            double secondNumber = string.IsNullOrEmpty(_currentInput)
                ? _calculator.StoredNumber
                : double.Parse(_currentInput, CultureInfo.InvariantCulture);

            // Форматування для історії
            string firstNum = _calculator.StoredNumber < 0
                ? $"({_calculator.StoredNumber})"
                : _calculator.StoredNumber.ToString(CultureInfo.InvariantCulture);

            string secondNum = secondNumber < 0
                ? $"({secondNumber})"
                : secondNumber.ToString(CultureInfo.InvariantCulture);

            char currentOperator = _calculator.PendingOperator;
            double result = _calculator.Calculate(secondNumber);

            if (_calculator.ErrorState)
            {
                DisplayAlert("Помилка", "Ділення на нуль неможливе", "OK");
                return;
            }

            _fullExpression = $"{firstNum} {currentOperator} {secondNum} =";
            _currentInput = result.ToString(CultureInfo.InvariantCulture);
            _isNewInput = true;
            UpdateDisplay();
        }
    }
}