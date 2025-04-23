namespace CalculatorCore.Tests
{
    public class IntegrationTests
    {
        [Fact]
        public void InputHandler_WithEngine_ComputesComplexExpression()
        {
            var engine = new CalculatorEngine();
            var formatter = new ExpressionFormatter();
            var input = new InputHandler(engine, formatter);

            // Симулюємо "(-5.5) * 2 ="
            input.HandleDigit("5");
            input.HandleDecimalPoint();
            input.HandleDigit("5");
            input.HandleToggleSign();
            input.HandleOperator('*');
            input.HandleDigit("2");
            input.HandleEquals();

            Assert.Equal("(-11)", input.CurrentInput);
            Assert.Contains("(-5.5) * 2 =", input.FullExpression);
        }
    }
}
