using CalculatorCore;
using CalculatorCore.Services;
using Microsoft.Extensions.Logging;

namespace CalculatorApp
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            // Реєстрація сервісів
            builder.Services
                .AddSingleton<ExpressionFormatter>() // Спочатку залежність
                .AddSingleton<ICalculatorEngine, CalculatorEngine>()
                .AddSingleton<IInputHandler, InputHandler>(); // Використовує ExpressionFormatter

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}