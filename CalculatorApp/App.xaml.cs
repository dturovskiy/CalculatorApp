namespace CalculatorApp
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new AppShell())
            {
                Width = 270,
                Height = 500,

                Title = "Calculator App"
            };
        }
    }
}