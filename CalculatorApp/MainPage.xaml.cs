using CalculatorApp.Services;

namespace CalculatorApp
{
    public partial class MainPage : ContentPage
    {
        private readonly IInputHandler _inputHandler;

        public MainPage()
        {
            InitializeComponent();
            _inputHandler = new InputHandler();
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
            throw new NotImplementedException();
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
    }
}