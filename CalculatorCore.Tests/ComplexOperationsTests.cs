namespace CalculatorCore.Tests
{
    public class ComplexOperationsTests
    {
        private readonly CalculatorEngine _calculator;
        private readonly InputHandler _inputHandler;

        public ComplexOperationsTests()
        {
            _calculator = new CalculatorEngine();
            _inputHandler = new InputHandler(_calculator, new ExpressionFormatter());
        }

        // === 1. Базові арифметичні операції ===
        [Theory]
        [InlineData("5", '+', "3", "8")]
        [InlineData("5", '-', "3", "2")]
        [InlineData("4", '*', "3", "12")]
        [InlineData("6", '/', "2", "3")]
        public void BasicOperations_ReturnsCorrectResult(string a, char op, string b, string expected)
        {
            _inputHandler.HandleDigit(a);
            _inputHandler.HandleOperator(op);
            _inputHandler.HandleDigit(b);
            _inputHandler.HandleEquals();
            Assert.Equal(expected, _inputHandler.CurrentInput);
        }

        // === 2. Операції з відсотками (A% op B) ===
        [Theory]
        [InlineData("5", '+', "1", "1.05")]  // 5% від 1 = 0.05 → 1 + 0.05 = 1.05
        [InlineData("5", '-', "1", "(-0.95)")]   // 5% від 1 = 0.05 → 1 - 0.05 = 0.95
        [InlineData("5", '*', "2", "0.1")]    // 5% від 2 = 0.1 → 2 * 0.1 = 0.1
        [InlineData("5", '/', "1", "0.05")]     // 5% від 1 = 0.05 → 1 / 0.05 = 20
        public void PercentFirstOperand_Operations_ReturnsCorrectResult(string a, char op, string b, string expected)
        {
            _inputHandler.HandleDigit(a);
            _inputHandler.HandlePercent();
            _inputHandler.HandleOperator(op);
            _inputHandler.HandleDigit(b);
            _inputHandler.HandleEquals();
            Assert.Equal(expected, _inputHandler.CurrentInput);
        }

        // === 3. Операції з відсотками (A op B%) ===
        [Theory]
        [InlineData("1", '+', "5", "1.05")]  // 1 + 5% від 1 = 1.05
        [InlineData("1", '-', "5", "0.95")]   // 1 - 5% від 1 = 0.95
        [InlineData("2", '*', "5", "0.1")]    // 2 * 5% = 0.1
        [InlineData("1", '/', "5", "20")]     // 1 / 5% = 20
        public void PercentSecondOperand_Operations_ReturnsCorrectResult(string a, char op, string b, string expected)
        {
            _inputHandler.HandleDigit(a);
            _inputHandler.HandleOperator(op);
            _inputHandler.HandleDigit(b);
            _inputHandler.HandlePercent();
            _inputHandler.HandleEquals();
            Assert.Equal(expected, _inputHandler.CurrentInput);
        }

        // === 4. Операції з від'ємними числами ===
        [Theory]
        [InlineData("5", '+', "3", "(-2)")]    // -5 + 3 = -2
        [InlineData("5", '-', "3", "(-8)")]    // -5 - 3 = -8
        [InlineData("5", '*', "3", "(-15)")]   // -5 * 3 = -15
        [InlineData("6", '/', "2", "(-3)")]    // -6 / 2 = -3
        public void NegativeFirstOperand_Operations_ReturnsCorrectResult(string a, char op, string b, string expected)
        {
            _inputHandler.HandleDigit(a);
            _inputHandler.HandleToggleSign();
            _inputHandler.HandleOperator(op);
            _inputHandler.HandleDigit(b);
            _inputHandler.HandleEquals();
            Assert.Equal(expected, _inputHandler.CurrentInput);
        }

        // === 5. Комбінації від'ємних чисел і відсотків ===
        [Theory]
        [InlineData("5", '+', "1", "0.95")]  // -5% від 1 = -0.05 → 1 + (-0.05) = 0.95
        [InlineData("5", '-', "1", "(-1.05)")]  // -5% від 1 = -0.05 → 1 - (-0.05) = 1.05
        public void NegativePercentCombinations_Operations_ReturnsCorrectResult(string a, char op, string b, string expected)
        {
            _inputHandler.HandleDigit(a);
            _inputHandler.HandleToggleSign();
            _inputHandler.HandlePercent();
            _inputHandler.HandleOperator(op);
            _inputHandler.HandleDigit(b);
            _inputHandler.HandleEquals();
            Assert.Equal(expected, _inputHandler.CurrentInput);
        }

        // === 6. Спеціальні випадки (A%, (-A%), тощо) ===
        [Theory]
        [InlineData("5", "0.05")]
        [InlineData("1", "0.01")]
        public void StandalonePercent_ReturnsCorrectValue(string input, string expected)
        {
            _inputHandler.HandleDigit(input);
            _inputHandler.HandlePercent();
            _inputHandler.HandleEquals();
            Assert.Equal(expected, _inputHandler.CurrentInput);
        }

        [Theory]
        [InlineData("5", "(-0.05)")]
        [InlineData("1", "(-0.01)")]
        public void NegativeStandalonePercent_ReturnsCorrectValue(string input, string expected)
        {
            _inputHandler.HandleDigit(input);
            _inputHandler.HandleToggleSign();
            _inputHandler.HandlePercent();
            _inputHandler.HandleEquals();
            Assert.Equal(expected, _inputHandler.CurrentInput);
        }
    }
}