namespace CalculatorCore.Services
{
    public interface ICalculatorEngine
    {
        double StoredNumber { get; }
        char PendingOperator { get; }
        bool HasPendingOperation { get; }
        bool ErrorState { get; }

        // Основні операції
        void SetOperation(double number, char operation);
        double Calculate(double secondNumber);
        void Reset();

        // Операції з відсотками
        double CalculateSimplePercent(double number);
        double CalculateWithPercent(double percent);
        double CalculatePercentOfNumber(double percent, double number);
    }
}