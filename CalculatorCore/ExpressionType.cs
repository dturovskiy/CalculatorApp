namespace CalculatorCore
{
    internal enum ExpressionType
    {
        RegularOperation,   // A + B
        PercentOperation,   // A + B%
        PercentOfNumber,    // A % B
        StandalonePercent,  // A%
        Unknown
    }
}
