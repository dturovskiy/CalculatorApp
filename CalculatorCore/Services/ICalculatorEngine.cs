namespace CalculatorCore.Services
{
    public interface ICalculatorEngine
    {
        double StoredNumber { get; set; }
        char PendingOperator { get; }
        bool ErrorState { get; }

        void SetOperation(double number, char operation);
        double Calculate(double secondNumber);
        void Reset();
    }
}