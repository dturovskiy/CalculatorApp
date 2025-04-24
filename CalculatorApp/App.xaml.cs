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
#if WINDOWS
                Width = 270,
                Height = 500,
#endif
                Title = "Calculator App"
            };
        }
    }
}