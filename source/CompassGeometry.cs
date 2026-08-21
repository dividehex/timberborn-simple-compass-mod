using System;

namespace SimpleCompass
{
    /// <summary>
    /// Pure math pulled out of <see cref="SimpleCompassWidget"/> so it can be unit
    /// tested without a running Unity/Timberborn process - nothing in here touches
    /// UnityEngine or Timberborn, on purpose.
    /// </summary>
    internal static class CompassGeometry
    {
        /// <summary>
        /// Degrees to rotate the needle so it keeps pointing at true north, given true
        /// north expressed in the camera's local right (x) / up (y) axes.
        /// </summary>
        public static float NeedleRotationDegrees(float localNorthX, float localNorthY)
        {
            float headingDegrees = (float)(Math.Atan2(localNorthX, localNorthY) * (180.0 / Math.PI));

            // Empirically, UI Toolkit's `rotate` spins counter-clockwise for positive
            // degrees on this build (opposite the usual CSS clockwise convention), so
            // the sign is flipped here to keep the needle's east/west correct.
            return -headingDegrees;
        }

        /// <summary>
        /// Clamps a proposed dial position so the dial stays fully within its parent's
        /// bounds, regardless of resolution or window size.
        /// </summary>
        public static (float Left, float Top) ClampPosition(
            float left, float top, float boundsWidth, float boundsHeight, float dialDiameter)
        {
            float maxLeft = Math.Max(0f, boundsWidth - dialDiameter);
            float maxTop = Math.Max(0f, boundsHeight - dialDiameter);
            return (Clamp(left, 0f, maxLeft), Clamp(top, 0f, maxTop));
        }

        private static float Clamp(float value, float min, float max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }
    }
}
