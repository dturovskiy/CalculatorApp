namespace CalculatorCore.Services
{
    internal enum ExpressionType
    {
        RegularOperation,   // A + B
        PercentOperation,   // A + B%
        PercentOfNumber,    // A % B
        PercentFirstOperation, // А% + В
        StandalonePercent,  // A%
        SingleNumber,       // A
        Unknown,
    }
}
