using Xunit;

namespace SimpleCompass.Tests
{
    public class CompassScaleTests
    {
        [Theory]
        [InlineData(100, 100)]
        [InlineData(50, 50)]
        [InlineData(200, 200)]
        [InlineData(0, 50)]
        [InlineData(-10, 50)]
        [InlineData(999, 200)]
        public void ClampPercent_KeepsValueWithinRange(int percent, int expected)
        {
            Assert.Equal(expected, CompassScale.ClampPercent(percent));
        }

        [Theory]
        [InlineData(100, 1f)]     // 100% is the base HUD-clock dial size (1.0x)
        [InlineData(50, 0.5f)]
        [InlineData(200, 2f)]
        [InlineData(999, 2f)]     // out-of-range input is clamped first
        public void UiScaleFor_ScalesTheBaseSize(int percent, float expectedScale)
        {
            Assert.Equal(expectedScale, CompassScale.UiScaleFor(percent), 5);
        }
    }
}
