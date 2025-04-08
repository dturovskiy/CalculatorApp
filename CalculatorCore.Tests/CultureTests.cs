//using System.Globalization;

//namespace CalculatorCore.Tests
//{
//    public class CultureTests
//    {
//        private readonly CalculatorEngine _engine = new();
//        private readonly InputHandler _inputHandler;

//        public CultureTests()
//        {
//            _inputHandler = new InputHandler(_engine);
//        }

//        [Theory]
//        [InlineData("en-US", "5.2", "+", "3.7", "8.9")]
//        [InlineData("uk-UA", "5.2", "+", "3.7", "8.9")]
//        public void HandleDecimal_WithDifferentCultures_UsesInvariant(string culture, string a, string op, string b, string expected)
//        {
//            // Arrange
//            var originalCulture = CultureInfo.CurrentCulture;
//            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture);

//            var engine = new CalculatorEngine();
//            var handler = new InputHandler(engine);

//            // Act (коректне введення з урахуванням крапки)
//            foreach (var ch in a)
//            {
//                if (ch == '.')
//                    handler.HandleDecimalPoint(); // Використовуємо метод для крапки
//                else
//                    handler.HandleDigit(ch.ToString());
//            }

//            handler.HandleOperator(op[0]);

//            foreach (var ch in b)
//            {
//                if (ch == '.')
//                    handler.HandleDecimalPoint();
//                else
//                    handler.HandleDigit(ch.ToString());
//            }

//            handler.HandleEquals();

//            // Assert
//            Assert.Equal(expected, handler.CurrentInput);

//            // Cleanup
//            CultureInfo.CurrentCulture = originalCulture;
//        }
//    }
//}