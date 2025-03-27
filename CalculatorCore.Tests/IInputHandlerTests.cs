using CalculatorCore.Services;
using Moq;

namespace CalculatorCore.Tests
{
    public class IInputHandlerTests
    {
        private readonly Mock<IInputHandler> _handlerMock;

        public IInputHandlerTests()
        {
            _handlerMock = new Mock<IInputHandler>();
        }

        [Fact]
        public void CurrentInput_ShouldReturnString()
        {
            // Arrange
            _handlerMock.SetupGet(x => x.CurrentInput).Returns("5");

            // Act & Assert
            Assert.Equal("5", _handlerMock.Object.CurrentInput);
        }

        [Theory]
        [InlineData("5", "5")]
        [InlineData("0", "0")]
        public void HandleDigit_ShouldProcessDigits(string input, string expected)
        {
            // Arrange
            _handlerMock.Setup(x => x.HandleDigit(It.IsAny<string>()))
                .Callback<string>(digit =>
                    _handlerMock.SetupGet(x => x.CurrentInput).Returns(digit));

            // Act
            _handlerMock.Object.HandleDigit(input);

            // Assert
            _handlerMock.Verify(x => x.HandleDigit(input), Times.Once);
            Assert.Equal(expected, _handlerMock.Object.CurrentInput);
        }

        [Fact]
        public void HandleOperator_ShouldUpdateExpression()
        {
            // Arrange
            var testExpression = "5 + ";
            _handlerMock.Setup(x => x.HandleOperator(It.IsAny<char>()))
                .Callback<char>(op =>
                    _handlerMock.SetupGet(x => x.FullExpression).Returns(testExpression));

            // Act
            _handlerMock.Object.HandleOperator('+');

            // Assert
            _handlerMock.Verify(x => x.HandleOperator('+'), Times.Once);
            Assert.Equal(testExpression, _handlerMock.Object.FullExpression);
        }

        [Fact]
        public void HandleDecimalPoint_ShouldAddDecimal()
        {
            // Arrange
            _handlerMock.Setup(x => x.HandleDecimalPoint())
                .Callback(() =>
                    _handlerMock.SetupGet(x => x.CurrentInput).Returns("0."));

            // Act
            _handlerMock.Object.HandleDecimalPoint();

            // Assert
            _handlerMock.Verify(x => x.HandleDecimalPoint(), Times.Once);
            Assert.Equal("0.", _handlerMock.Object.CurrentInput);
        }

        [Fact]
        public void HandleToggleSign_ShouldToggleNumberSign()
        {
            // Arrange
            _handlerMock.Setup(x => x.HandleToggleSign())
                .Callback(() =>
                    _handlerMock.SetupGet(x => x.CurrentInput).Returns("-5"));

            // Act
            _handlerMock.Object.HandleToggleSign();

            // Assert
            _handlerMock.Verify(x => x.HandleToggleSign(), Times.Once);
            Assert.Equal("-5", _handlerMock.Object.CurrentInput);
        }

        [Theory]
        [InlineData(true)]  // Full clear
        [InlineData(false)] // Partial clear
        public void HandleClear_ShouldResetState(bool fullClear)
        {
            // Arrange
            _handlerMock.Setup(x => x.HandleClear(fullClear))
                .Callback(() =>
                {
                    _handlerMock.SetupGet(x => x.CurrentInput).Returns("");
                    _handlerMock.SetupGet(x => x.IsNewInput).Returns(true);
                });

            // Act
            _handlerMock.Object.HandleClear(fullClear);

            // Assert
            _handlerMock.Verify(x => x.HandleClear(fullClear), Times.Once);
            Assert.Equal("", _handlerMock.Object.CurrentInput);
            Assert.True(_handlerMock.Object.IsNewInput);
        }

        [Fact]
        public void HandleEquals_ShouldCompleteExpression()
        {
            // Arrange
            var testExpression = "5 + 3 = 8";
            _handlerMock.Setup(x => x.HandleEquals())
                .Callback(() =>
                {
                    _handlerMock.SetupGet(x => x.FullExpression).Returns(testExpression);
                    _handlerMock.SetupGet(x => x.IsNewInput).Returns(true);
                });

            // Act
            _handlerMock.Object.HandleEquals();

            // Assert
            _handlerMock.Verify(x => x.HandleEquals(), Times.Once);
            Assert.Equal(testExpression, _handlerMock.Object.FullExpression);
            Assert.True(_handlerMock.Object.IsNewInput);
        }

        [Fact]
        public void Reset_ShouldClearAllState()
        {
            // Arrange
            _handlerMock.Setup(x => x.Reset())
                .Callback(() =>
                {
                    _handlerMock.SetupGet(x => x.CurrentInput).Returns("");
                    _handlerMock.SetupGet(x => x.FullExpression).Returns("");
                    _handlerMock.SetupGet(x => x.IsNewInput).Returns(true);
                    _handlerMock.SetupGet(x => x.ErrorState).Returns(false);
                });

            // Act
            _handlerMock.Object.Reset();

            // Assert
            _handlerMock.Verify(x => x.Reset(), Times.Once);
            Assert.Equal("", _handlerMock.Object.CurrentInput);
            Assert.Equal("", _handlerMock.Object.FullExpression);
            Assert.True(_handlerMock.Object.IsNewInput);
            Assert.False(_handlerMock.Object.ErrorState);
        }
    }
}