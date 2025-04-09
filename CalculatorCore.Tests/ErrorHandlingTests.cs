namespace CalculatorCore.Tests
{
    public class ErrorHandlingTests
    {
        [Fact]
        public void HandleDigit_InvalidChars_IgnoresInput()
        {
            var input = new InputHandler(new CalculatorEngine(), new ExpressionFormatter());
            input.HandleDigit("a");  // Некоректний символ
            Assert.Equal("", input.CurrentInput);
        }

        [Fact]
        public void HandleDecimalPoint_Duplicate_IgnoresSecondPoint()
        {
            var input = new InputHandler(new CalculatorEngine(), new ExpressionFormatter());
            input.HandleDecimalPoint();
            input.HandleDecimalPoint();  // Друга крапка ігнорується
            Assert.Equal("0.", input.CurrentInput);
        }
    }
}
