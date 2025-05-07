using CalculatorCore;
using CalculatorCore.Services;
using System.Diagnostics;

namespace CalculatorApp
{
    public partial class MainPage : ContentPage
    {
        private readonly InputHandler _inputHandler;
        private double defaultFontSize = 55; // Початковий розмір шрифту
        private double maxLabelWidth = 250; // Узгоджено з WidthRequest
        private const int maxTextLength = 12; // Максимальна кількість символів

        public MainPage(IInputHandler inputHandler)
        {
            InitializeComponent();
            _inputHandler = (InputHandler)inputHandler;
            UpdateDisplay();
            SizeChanged += OnPageSizeChanged; // Підписка на зміну розміру
        }

        private void OnDigitClicked(object sender, EventArgs e)
        {
            var button = (Button)sender;
            if (_inputHandler.CurrentInput.Length < maxTextLength)
            {
                _inputHandler.HandleDigit(button.Text);
                UpdateDisplay();
            }
        }

        private void OnOperatorClicked(object sender, EventArgs e)
        {
            var button = (Button)sender;
            char op = button.Text[0] == '×' ? '*' : button.Text[0];
            Debug.WriteLine($"Operator clicked: {button.Text[0]} -> {op}");
            _inputHandler.HandleOperator(button.Text[0]);
            UpdateDisplay();
        }

        private void OnDecimalPointClicked(object sender, EventArgs e)
        {
            if (_inputHandler.CurrentInput.Length < maxTextLength)
            {
                _inputHandler.HandleDecimalPoint();
                UpdateDisplay();
            }
        }

        private void OnToggleSignClicked(object sender, EventArgs e)
        {
            _inputHandler.HandleToggleSign();
            UpdateDisplay();
        }

        private void OnACClicked(object sender, EventArgs e)
        {
            bool isFullClear = string.IsNullOrEmpty(_inputHandler.CurrentInput) ||
                              _inputHandler.CurrentInput == "0" ||
                              _inputHandler.IsNewInput;
            _inputHandler.HandleClear(isFullClear);
            UpdateDisplay();
        }

        private void OnPercentClicked(object sender, EventArgs e)
        {
            _inputHandler.HandlePercent();
            UpdateDisplay();
        }

        private void OnEqualsClicked(object sender, EventArgs e)
        {
            _inputHandler.HandleEquals();
            UpdateDisplay();
        }

        private void UpdateDisplay()
        {
            ACButton.Text = string.IsNullOrEmpty(_inputHandler.CurrentInput) ||
                            _inputHandler.ErrorState ||
                            _inputHandler.IsNewInput ||
                            _inputHandler.CurrentInput == "0"
                ? "AC"
                : "←";

            DisplayLabel.Text = string.IsNullOrEmpty(_inputHandler.CurrentInput)
                ? "0"
                : _inputHandler.CurrentInput;

            HistoryLabel.Text = _inputHandler.FullExpression;

            UpdateDisplayFontSize(); // Оновлення розміру шрифту
        }

        private void UpdateDisplayFontSize()
        {
            var label = DisplayLabel;
            int textLength = label.Text.Length;

            // Динамічне зменшення шрифту залежно від довжини тексту
            if (textLength > 8)
            {
                double scale = 8.0 / textLength; // Зменшуємо пропорційно
                label.FontSize = defaultFontSize * scale;
            }
            else
            {
                label.FontSize = defaultFontSize;
            }
        }

        private void OnPageSizeChanged(object? sender, EventArgs e)
        {
            var page = (ContentPage)sender!;
            double width = page.Width;
            double height = page.Height;

            if (width > height)
            {
                // Ландшафтний режим
                HistoryLabel.FontSize = 24;
                defaultFontSize = 40;
                maxLabelWidth = 200;
                DisplayLabel.WidthRequest = 200;
                DisplayLabel.MaximumWidthRequest = 200;
            }
            else
            {
                // Портретний режим
                HistoryLabel.FontSize = 35;
                defaultFontSize = 55;
                maxLabelWidth = 280;
                DisplayLabel.WidthRequest = 250;
                DisplayLabel.MaximumWidthRequest = 250;
            }

            UpdateDisplayFontSize(); // Оновлюємо шрифт після зміни режиму
        }
    }
}