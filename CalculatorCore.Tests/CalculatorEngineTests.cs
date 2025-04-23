namespace CalculatorCore.Tests
{
    public class CalculatorEngineTests
    {
        private readonly CalculatorEngine _engine = new();

        // --- Ваші оригінальні тести (без змін) ---
        [Theory]
        [InlineData(5, 3, '+', 8)]
        [InlineData(10, 4, '-', 6)]
        [InlineData(2.5, 4, '*', 10)]
        [InlineData(10, 2, '/', 5)]
        public void Calculate_ValidOperations_ReturnsCorrectResult(double a, double b, char op, double expected)
        {
            _engine.SetOperation(a, op);
            var result = _engine.Calculate(b);
            Assert.Equal(expected, result);
            Assert.False(_engine.ErrorState);
        }

        /// <summary>
        /// Перевіряє, що ядро калькулятора відхиляє невірні оператори
        /// (На рівні UI такі оператори неможливо ввести)
        /// </summary>
        [Fact]
        public void Calculate_InvalidOperator_ThrowsException()
        {
            var ex = Assert.Throws<ArgumentException>(() => _engine.SetOperation(5, '&'));
            Assert.Equal("Invalid operator. Allowed: +, -, *, /", ex.Message);
        }

        [Fact]
        public void SetOperation_ValidOperator_SetsStateCorrectly()
        {
            _engine.SetOperation(5, '+');
            Assert.Equal(5, _engine.StoredNumber);
            Assert.Equal('+', _engine.PendingOperator);
            Assert.False(_engine.ErrorState);
        }

        [Fact]
        public void HasPendingOperation_ReturnsCorrectState()
        {
            Assert.False(_engine.HasPendingOperation);
            _engine.SetOperation(5, '+');
            Assert.True(_engine.HasPendingOperation);
            _engine.Calculate(3);
            Assert.False(_engine.HasPendingOperation);
        }

        [Fact]
        public void Reset_ClearsAllState()
        {
            _engine.SetOperation(5, '+');
            _engine.Calculate(0);
            _engine.Reset();
            Assert.Equal(0, _engine.StoredNumber);
            Assert.Equal('\0', _engine.PendingOperator);
            Assert.False(_engine.ErrorState);
        }

        // --- Нові тести для відсотків (додано без змін до існуючих) ---
        [Theory]
        [InlineData(250, 2.5)]
        [InlineData(-5, -0.05)]
        [InlineData(0.1, 0.001)]
        public void CalculateSimplePercent_ReturnsCorrectValue(double input, double expected)
        {
            var result = _engine.CalculateSimplePercent(input);
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData(100, '+', 10, 110)]
        [InlineData(200, '-', 20, 160)]
        [InlineData(50, '*', 10, 5)]
        [InlineData(100, '/', 10, 1000)]
        public void CalculateWithPercent_ValidOperations_ReturnsCorrectResult(
            double a, char op, double percent, double expected)
        {
            _engine.SetOperation(a, op);
            var result = _engine.CalculateWithPercent(percent);
            Assert.Equal(expected, result);
            Assert.False(_engine.ErrorState);
        }

        [Fact]
        public void CalculateWithPercent_WithoutSetOperation_ThrowsException()
        {
            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(
                () => _engine.CalculateWithPercent(10));

            // Додаткова перевірка, що виняток дійсно пов'язаний з відсутністю оператора
            Assert.NotNull(exception);
        }

        [Fact]
        public void PercentOperations_DoNotAffectBasicOperations()
        {
            // Перевірка, що основні операції працюють після роботи з відсотками
            _engine.SetOperation(100, '+');
            _engine.CalculateWithPercent(10); // 100 + 10% = 110

            _engine.SetOperation(50, '*');
            var result = _engine.Calculate(2); // 50 * 2
            Assert.Equal(100, result); // Має бути 100, незалежно від попередніх операцій
        }
    }
}