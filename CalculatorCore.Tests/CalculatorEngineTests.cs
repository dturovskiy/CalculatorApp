namespace CalculatorCore.Tests
{
    public class CalculatorEngineTests
    {
        private readonly CalculatorEngine _engine = new();

        [Theory]
        [InlineData(5, 3, '+', 8)]
        [InlineData(10, 4, '-', 6)]
        [InlineData(2.5, 4, '*', 10)]
        [InlineData(10, 2, '/', 5)]
        public void Calculate_ValidOperations_ReturnsCorrectResult(
            double a, double b, char op, double expected)
        {
            // Arrange
            _engine.SetOperation(a, op);

            // Act
            var result = _engine.Calculate(b);

            // Assert
            Assert.Equal(expected, result);
            Assert.False(_engine.ErrorState);
            Assert.Equal('\0', _engine.PendingOperator); // Оператор скидається
        }

        [Fact]
        public void Calculate_DivideByZero_SetsErrorState()
        {
            // Arrange
            _engine.SetOperation(5, '/');

            // Act
            var result = _engine.Calculate(0);

            // Assert
            Assert.True(_engine.ErrorState);
            Assert.Equal(0, result);
        }

        [Fact]
        public void Calculate_InvalidOperator_ThrowsException()
        {
            // Arrange
            _engine.SetOperation(5, '+'); // Спочатку валідний оператор

            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() =>
                _engine.SetOperation(5, '&')); // Невірний оператор

            Assert.Equal("Невірний оператор", ex.Message);
        }

        [Fact]
        public void SetOperation_ValidOperator_SetsStateCorrectly()
        {
            // Act
            _engine.SetOperation(5, '+');

            // Assert
            Assert.Equal(5, _engine.StoredNumber);
            Assert.Equal('+', _engine.PendingOperator);
            Assert.False(_engine.ErrorState);
        }

        [Fact]
        public void HasPendingOperation_ReturnsCorrectState()
        {
            // Початковий стан
            Assert.False(_engine.HasPendingOperation);

            // Після встановлення оператора
            _engine.SetOperation(5, '+');
            Assert.True(_engine.HasPendingOperation);

            // Після обчислення
            _engine.Calculate(3);
            Assert.False(_engine.HasPendingOperation);
        }

        [Fact]
        public void Reset_ClearsAllState()
        {
            // Arrange
            _engine.SetOperation(5, '+');
            _engine.Calculate(0); // Генеруємо помилку (ділення на 0)

            // Act
            _engine.Reset();

            // Assert
            Assert.Equal(0, _engine.StoredNumber);
            Assert.Equal('\0', _engine.PendingOperator);
            Assert.False(_engine.ErrorState);
        }

        [Fact]
        public void Calculate_WithoutSetOperation_ReturnsZero()
        {
            // Act
            var result = _engine.Calculate(5);

            // Assert
            Assert.Equal(0, result);
            Assert.False(_engine.ErrorState);
        }

        [Fact]
        public void ErrorState_ResetsOnNewOperation()
        {
            // Arrange
            var engine = new CalculatorEngine();

            // Act: Створюємо помилку (ділення на 0)
            engine.SetOperation(5, '/');
            engine.Calculate(0);

            // Assert: Перевіряємо, що помилка встановлена
            Assert.True(engine.ErrorState);

            // Act: Встановлюємо нову операцію
            engine.SetOperation(3, '+');

            // Assert: Помилка має скинутися
            Assert.False(engine.ErrorState);
            Assert.Equal(3, engine.StoredNumber);
            Assert.Equal('+', engine.PendingOperator);
        }
    }
}