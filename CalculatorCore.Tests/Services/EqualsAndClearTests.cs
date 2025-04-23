namespace CalculatorCore.Tests
{
    public class EqualsAndClearTests : CalculatorTestBase
    {
        public EqualsAndClearTests() : base(useRealEngine: false)
        {
        }

        // ===== Тести для "=" (дорівнює) =====
        [Fact]
        public void HandleEquals_WithSimpleAddition_CalculatesResult()
        {
            // Arrange
            CalculatorMock.Setup(x => x.Calculate(3)).Returns(5);
            EnterExpression("2+3");

            // Act
            InputHandler.HandleEquals();

            // Assert
            Assert.Equal("5", InputHandler.CurrentInput);
            Assert.Equal("2 + 3 =", InputHandler.FullExpression);
        }

        [Fact]
        public void HandleEquals_WithoutExpression_KeepsValue()
        {
            // Arrange
            EnterDigits("42");

            // Act
            InputHandler.HandleEquals();

            // Assert
            Assert.Equal("42", InputHandler.CurrentInput);
            Assert.Equal("42 =", InputHandler.FullExpression);
        }

        [Fact]
        public void HandleEquals_AfterErrorState_DoesNothing()
        {
            // Arrange
            CalculatorMock.Setup(x => x.ErrorState).Returns(true);
            EnterDigits("5");

            // Act
            InputHandler.HandleEquals();

            // Assert
            Assert.Equal("5", InputHandler.CurrentInput);
        }

        [Fact]
        public void HandleEquals_WithDecimalNumbers_FormatsCorrectly()
        {
            // Arrange
            CalculatorMock.Setup(x => x.Calculate(0.3)).Returns(0.6);
            EnterExpression("0.3+0.3");

            // Act
            InputHandler.HandleEquals();

            // Assert
            Assert.Equal("0.6", InputHandler.CurrentInput);
        }

        // ===== Тести для "AC" (очищення) =====
        [Fact]
        public void HandleClear_FullClear_ResetsAllState()
        {
            // Arrange
            EnterExpression("10+20");

            // Act
            InputHandler.HandleClear(fullClear: true);

            // Assert
            Assert.Equal("", InputHandler.CurrentInput);
            Assert.Equal("", InputHandler.FullExpression);
            Assert.True(InputHandler.IsNewInput);
        }

        [Fact]
        public void HandleClear_PartialClear_RemovesLastCharacter()
        {
            // Arrange
            EnterDigits("123");

            // Act
            InputHandler.HandleClear(fullClear: false);

            // Assert
            Assert.Equal("12", InputHandler.CurrentInput);
        }

        [Fact]
        public void HandleClear_AfterErrorState_ResetsError()
        {
            // Arrange
            CalculatorMock.Setup(x => x.ErrorState).Returns(true);

            // Act
            InputHandler.HandleClear(fullClear: true);

            // Assert
            Assert.False(InputHandler.ErrorState);
        }

        // ===== Тести для комбінованих операцій =====
        [Fact]
        public void HandleClear_FullThenPartial_WorksCorrectly()
        {
            // Arrange
            EnterExpression("15+37");
            InputHandler.HandleClear(fullClear: true);

            // Act
            InputHandler.HandleClear(fullClear: false);

            // Assert
            Assert.Equal("", InputHandler.CurrentInput);
        }

        [Fact]
        public void HandleEquals_ThenPartialClear_StartsNewInput()
        {
            // Arrange
            CalculatorMock.Setup(x => x.Calculate(2)).Returns(4);
            EnterExpression("2+2=");

            // Act
            InputHandler.HandleClear(fullClear: false);

            // Assert
            Assert.Equal("", InputHandler.CurrentInput);
            Assert.True(InputHandler.IsNewInput);
        }

        [Fact]
        public void HandleClear_OnDecimalNumber_ResetsDecimalFlag()
        {
            // Arrange
            EnterDigits("3.14");
            InputHandler.HandleClear(fullClear: false); // Видаляємо 4
            InputHandler.HandleClear(fullClear: false); // Видаляємо .

            // Act
            InputHandler.HandleDecimalPoint(); // Має знову дозволити крапку

            // Assert
            Assert.Equal("3.", InputHandler.CurrentInput);
        }
    }
}