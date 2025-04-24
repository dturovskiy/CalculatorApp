using CalculatorCore;
using CalculatorCore.Services;

namespace CalculatorApp
{
    public partial class MainPage : ContentPage
    {
        private readonly InputHandler _inputHandler;

        public MainPage(IInputHandler inputHandler)
        {
            InitializeComponent();
            _inputHandler = (InputHandler)inputHandler;
            UpdateDisplay();
        }

        private void OnDigitClicked(object sender, EventArgs e)
        {
            var button = (Button)sender;
            _inputHandler.HandleDigit(button.Text);
            UpdateDisplay();
        }

        private void OnOperatorClicked(object sender, EventArgs e)
        {
            var button = (Button)sender;
            _inputHandler.HandleOperator(button.Text[0]);
            UpdateDisplay();
        }

        private void OnDecimalPointClicked(object sender, EventArgs e)
        {
            _inputHandler.HandleDecimalPoint();
            UpdateDisplay();
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
        }

        private void OnPageSizeChanged(object sender, EventArgs e)
        {
            // Тут можна додати логіку для адаптації під різні розміри екрану
            var page = (ContentPage)sender;
            double width = page.Width;
            double height = page.Height;

            if (width > height)
            {
                // Ландшафтний режим
                HistoryLabel.FontSize = 16;
                DisplayLabel.FontSize = 24;
            }
            else
            {
                // Портретний режим
                HistoryLabel.FontSize = 20;
                DisplayLabel.FontSize = 32;
            }
        }
    }
}