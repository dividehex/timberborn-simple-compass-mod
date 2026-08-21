using Xunit;

namespace SimpleCompass.Tests
{
    public class CompassGeometryTests
    {
        [Theory]
        [InlineData(0f, 1f, 0f)]     // camera facing north -> needle points straight up
        [InlineData(1f, 0f, -90f)]   // north is to the camera's right -> needle rotates -90
        [InlineData(-1f, 0f, 90f)]   // north is to the camera's left -> needle rotates +90
        [InlineData(1f, 1f, -45f)]   // north is halfway between up and right
        public void NeedleRotationDegrees_TracksLocalNorth(float localNorthX, float localNorthY, float expectedDegrees)
        {
            float actual = CompassGeometry.NeedleRotationDegrees(localNorthX, localNorthY);

            Assert.Equal(expectedDegrees, actual, 3);
        }

        [Fact]
        public void ClampPosition_LeavesPositionUnchanged_WhenWithinBounds()
        {
            (float left, float top) = CompassGeometry.ClampPosition(50f, 60f, 800f, 600f, 90f);

            Assert.Equal(50f, left);
            Assert.Equal(60f, top);
        }

        [Fact]
        public void ClampPosition_ClampsToFarEdge_WhenPastRightOrBottom()
        {
            (float left, float top) = CompassGeometry.ClampPosition(10000f, 10000f, 800f, 600f, 90f);

            Assert.Equal(800f - 90f, left);
            Assert.Equal(600f - 90f, top);
        }

        [Fact]
        public void ClampPosition_ClampsToZero_WhenNegative()
        {
            (float left, float top) = CompassGeometry.ClampPosition(-50f, -50f, 800f, 600f, 90f);

            Assert.Equal(0f, left);
            Assert.Equal(0f, top);
        }

        [Fact]
        public void ClampPosition_NeverGoesNegative_WhenBoundsSmallerThanDial()
        {
            (float left, float top) = CompassGeometry.ClampPosition(500f, 500f, 50f, 50f, 90f);

            Assert.Equal(0f, left);
            Assert.Equal(0f, top);
        }
    }
}
