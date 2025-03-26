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
                _currentInput = digit;
                _isNewInput = false;
                _hasDecimalPoint = false;
            }
            else if (!(_currentInput == "0" && digit == "0"))
            {
                _currentInput += digit;
            }

            UpdateDisplay();
        }

        private void OnToggleSignClicked(object sender, EventArgs e)
        {
            if (_calculator.ErrorState || string.IsNullOrEmpty(_currentInput))
                return;

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

                double number = double.Parse(_currentInput, NumberStyles.Any, CultureInfo.InvariantCulture);
                _calculator.SetOperation(number, newOperator);
                _fullExpression = $"{number.ToString(CultureInfo.InvariantCulture)} {newOperator} ";
            }
            else if (_calculator.PendingOperator != '\0')
            {
                _calculator.SetOperation(_calculator.StoredNumber, newOperator);
                _fullExpression = $"{_calculator.StoredNumber.ToString(CultureInfo.InvariantCulture)} {newOperator} ";
            }

            _isNewInput = true;
            _currentInput = "";
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
                : double.Parse(_currentInput, NumberStyles.Any, CultureInfo.InvariantCulture);

            char currentOperator = _calculator.PendingOperator;
            double result = _calculator.Calculate(secondNumber);

            if (_calculator.ErrorState)
            {
                DisplayAlert("Помилка", "Ділення на нуль неможливе", "OK");
                return;
            }

            _fullExpression = $"{_calculator.StoredNumber.ToString(CultureInfo.InvariantCulture)} {currentOperator} {secondNumber.ToString(CultureInfo.InvariantCulture)} =";
            _currentInput = result.ToString(CultureInfo.InvariantCulture);
            _isNewInput = true;
            UpdateDisplay();
        }
    }
}