namespace CalculatorCore.Services;

public interface IInputHandler
{
    string CurrentInput { get; }
    string FullExpression { get; }
    bool IsNewInput { get; }
    bool ErrorState { get; }

    void HandleDigit(string digit);
    void HandleOperator(char op);
    void HandleDecimalPoint();
    void HandleToggleSign();
    void HandleClear(bool fullClear);
    void HandleEquals();
    void Reset();
}