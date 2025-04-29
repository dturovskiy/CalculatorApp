namespace CalculatorCore.Services
{
    internal enum ExpressionType
    {
        RegularOperation,   // A + B
        PercentOperation,   // A + B%
        PercentOfNumber,    // A % B
        StandalonePercent,  // A%
        SingleNumber,       // A
        Unknown,
    }
}
