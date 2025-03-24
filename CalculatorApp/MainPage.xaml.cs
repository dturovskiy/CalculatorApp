namespace CalculatorApp
{
    public partial class MainPage : ContentPage
    {
        private string _currentInput = ""; // Поточне число на екрані введення (порожнє після оператора)
        private string _fullExpression = ""; // Весь вираз у верхньому полі
        private string _pendingOperator = ""; // Останній вибраний оператор
        private double _storedNumber = 0; // Перше число для операції
        private bool _isNewInput = true; // Позначає, чи вводиться нове число після оператора
        private double _lastNumber = 0; // Останнє число для повторного обчислення
        private string _lastOperator = ""; // Остання операція для повторного обчислення
        private bool _errorState = false; // Чи сталася помилка (наприклад, ділення на нуль)

        public MainPage()
        {
            InitializeComponent();
            UpdateDisplay();
        }

        // Обробка цифр (0-9)
        private void OnDigitClicked(object sender, EventArgs e)
        {
            if (_errorState) return; // Забороняємо введення після помилки

            var button = (Button)sender;
            var digit = button.Text;

            if (_isNewInput)
            {
                _currentInput = digit;
                _isNewInput = false;
            }
            else
            {
                _currentInput += digit;
            }

            UpdateDisplay();
        }

        // Обробка кнопки "+/-" для зміни знака числа
        private void OnToggleSignClicked(object sender, EventArgs e)
        {
            if (_errorState || string.IsNullOrEmpty(_currentInput)) return;

            if (_currentInput.StartsWith("-"))
                _currentInput = _currentInput.Substring(1);
            else
                _currentInput = "-" + _currentInput;

            UpdateDisplay();
        }

        // Обробка операторів (+, -, *, /)
        private void OnOperatorClicked(object sender, EventArgs e)
        {
            if (_errorState) return; // Не дозволяємо вводити оператори після помилки

            var button = (Button)sender;
            var newOperator = button.Text;

            if (!string.IsNullOrEmpty(_currentInput)) // Якщо є число, обчислюємо
            {
                if (_pendingOperator != "")
                {
                    OnEqualsClicked(this, EventArgs.Empty); // Замість null передаємо коректні пусті значення
                }

                _storedNumber = double.Parse(_currentInput);
                _pendingOperator = newOperator;
                _fullExpression = $"{_storedNumber} {newOperator} ";
                _isNewInput = true;
            }
            else // Дозволяємо змінювати оператор без повторного введення числа
            {
                _pendingOperator = newOperator;
                _fullExpression = $"{_storedNumber} {newOperator} ";
            }

            _currentInput = ""; // Очищуємо поле введення
            UpdateDisplay();
        }

        // Обробка кнопки "="
        private void OnEqualsClicked(object sender, EventArgs e)
        {
            if (_errorState) return;

            // Якщо немає оператора або другого числа — ігноруємо "="
            if (_pendingOperator == "" || string.IsNullOrEmpty(_currentInput))
            {
                return;
            }

            double secondNumber = double.Parse(_currentInput);
            double result = 0;

            switch (_pendingOperator)
            {
                case "+": result = _storedNumber + secondNumber; break;
                case "-": result = _storedNumber - secondNumber; break;
                case "*": result = _storedNumber * secondNumber; break;
                case "/":
                    if (secondNumber == 0)
                    {
                        DisplayAlert("Помилка", "Ділення на нуль неможливе", "OK");
                        _errorState = true;
                        return;
                    }
                    result = _storedNumber / secondNumber;
                    break;
            }

            _fullExpression = $"{_storedNumber} {_pendingOperator} {secondNumber} =";
            _currentInput = result.ToString();
            _storedNumber = result;
            _pendingOperator = "";
            _isNewInput = true;

            UpdateDisplay();
        }

        // Обробка кнопки "C" (очищення)
        private void OnClearClicked(object sender, EventArgs e)
        {
            _currentInput = "";
            _storedNumber = 0;
            _pendingOperator = "";
            _fullExpression = "";
            _isNewInput = true;
            _lastNumber = 0;
            _lastOperator = "";
            _errorState = false;

            UpdateDisplay();
        }

        // Оновлення відображення
        private void UpdateDisplay()
        {
            DisplayLabel.Text = string.IsNullOrEmpty(_currentInput) ? "0" : _currentInput;
            HistoryLabel.Text = _fullExpression;
        }
    }
}