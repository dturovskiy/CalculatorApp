namespace CalculatorCore.Tests
{
    public class CalculatorEngineTests
    {
        private readonly CalculatorEngine _calculator;

        public CalculatorEngineTests()
        {
            _calculator = new CalculatorEngine();
        }

        // ============= SetOperation =============
        [Theory]
        [InlineData('+')]
        [InlineData('-')]
        [InlineData('*')]
        [InlineData('/')]
        public void SetOperation_ValidOperator_SetsOperation(char op)
        {
            // Act
            _calculator.SetOperation(5, op);

            // Assert
            Assert.Equal(5, _calculator.StoredNumber);
            Assert.Equal(op, _calculator.PendingOperator);
            Assert.False(_calculator.ErrorState);
        }

        [Fact]
        public void SetOperation_InvalidOperator_ThrowsException()
        {
            // Arrange
            char invalidOp = '%';

            // Act & Assert
            Assert.Throws<ArgumentException>(() => _calculator.SetOperation(5, invalidOp));
        }

        // ============= Calculate =============
        [Theory]
        [InlineData(2, '+', 3, 5)]
        [InlineData(5, '-', 3, 2)]
        [InlineData(4, '*', 3, 12)]
        [InlineData(10, '/', 2, 5)]
        public void Calculate_ValidOperations_ReturnsCorrectResult(double first, char op, double second, double expected)
        {
            // Arrange
            _calculator.SetOperation(first, op);

            // Act
            double result = _calculator.Calculate(second);

            // Assert
            Assert.Equal(expected, result);
            Assert.False(_calculator.HasPendingOperation);
        }

        [Fact]
        public void Calculate_DivideByZero_ThrowsException()
        {
            // Arrange
            _calculator.SetOperation(10, '/');

            // Act & Assert
            var exception = Assert.Throws<DivideByZeroException>(() => _calculator.Calculate(0));
            Assert.Equal("Division by zero is not allowed", exception.Message);
            Assert.True(_calculator.ErrorState); // Тепер тест буде проходити
        }

        [Fact]
        public void Calculate_NoPendingOperation_ThrowsException()
        {
            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => _calculator.Calculate(5));
        }

        // ============= Percent Calculations =============
        [Theory]
        [InlineData(100, 10, 10)]  // 10% of 100 = 10
        [InlineData(200, 50, 100)] // 50% of 200 = 100
        public void CalculatePercentOfNumber_CalculatesCorrectly(double number, double percent, double expected)
        {
            // Act
            double result = _calculator.CalculatePercentOfNumber(percent, number);

            // Assert
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData(100, '+', 10, 110)] // 100 + 10% = 110
        [InlineData(100, '-', 10, 90)]  // 100 - 10% = 90
        [InlineData(100, '*', 10, 10)]  // 100 * 10% = 10
        [InlineData(100, '/', 10, 1000)] // 100 / 10% = 1000
        public void CalculateWithPercent_ValidOperations_ReturnsCorrectResult(
            double number, char op, double percent, double expected)
        {
            // Arrange
            _calculator.SetOperation(number, op);

            // Act
            double result = _calculator.CalculateWithPercent(percent);

            // Assert
            Assert.Equal(expected, result);
            Assert.False(_calculator.HasPendingOperation);
        }

        [Fact]
        public void CalculateWithPercent_DivideByZeroPercent_ThrowsException()
        {
            // Arrange
            _calculator.SetOperation(100, '/');

            // Act & Assert
            Assert.Throws<DivideByZeroException>(() => _calculator.CalculateWithPercent(0));
            Assert.True(_calculator.ErrorState);
        }

        [Fact]
        public void CalculateWithPercent_NoPendingOperation_ThrowsException()
        {
            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => _calculator.CalculateWithPercent(10));
        }

        // ============= Reset =============
        [Fact]
        public void Reset_ClearsCalculatorState()
        {
            // Arrange
            _calculator.SetOperation(5, '+');
            _calculator.Calculate(3); // Error state could be set

            // Act
            _calculator.Reset();

            // Assert
            Assert.Equal(0, _calculator.StoredNumber);
            Assert.Equal('\0', _calculator.PendingOperator);
            Assert.False(_calculator.ErrorState);
        }

        // ============= Simple Percent =============
        [Theory]
        [InlineData(100, 1)]
        [InlineData(50, 0.5)]
        public void CalculateSimplePercent_CalculatesCorrectly(double number, double expected)
        {
            // Act
            double result = _calculator.CalculateSimplePercent(number);

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void CalculatorEngine_Division_ReturnsCorrectResult()
        {
            var engine = new CalculatorEngine();
            engine.SetOperation(10, '/');
            double result = engine.Calculate(2);
            Assert.Equal(5, result); // 10 / 2 = 5
        }
    }
}
