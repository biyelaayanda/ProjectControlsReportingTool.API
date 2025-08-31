using Xunit;

namespace ProjectControlsReportingTool.API.Tests.UnitTests
{
    /// <summary>
    /// Simple test to verify the test infrastructure is working
    /// </summary>
    public class BasicTests
    {
        [Fact]
        public void BasicTest_Always_Passes()
        {
            // Arrange
            var expected = true;
            
            // Act
            var actual = true;
            
            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void BasicMath_Addition_WorksCorrectly()
        {
            // Arrange
            var a = 2;
            var b = 3;
            var expected = 5;
            
            // Act
            var actual = a + b;
            
            // Assert
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(1, 1, 2)]
        [InlineData(2, 3, 5)]
        [InlineData(-1, 1, 0)]
        [InlineData(0, 0, 0)]
        public void BasicMath_Addition_MultipleValues(int a, int b, int expected)
        {
            // Act
            var actual = a + b;
            
            // Assert
            Assert.Equal(expected, actual);
        }
    }
}
