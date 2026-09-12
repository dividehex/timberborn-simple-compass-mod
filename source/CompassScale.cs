namespace SimpleCompass
{
    /// <summary>
    /// The compass size setting's value space, kept free of Unity/Timberborn so it can
    /// be unit tested. The setting is a whole-number percentage, where 100% is the
    /// compass's base size: the HUD clock's dial (GameMiscStyle.uss .clock-panel is
    /// 61px) at 1.0x.
    /// </summary>
    internal static class CompassScale
    {
        public const int MinPercent = 50;
        public const int MaxPercent = 200;
        public const int DefaultPercent = 100;

        // The multiplier at 100%: the compass's base (61px clock-sized) dimensions,
        // unscaled.
        private const float ScaleAtDefaultPercent = 1.0f;

        public static int ClampPercent(int percent)
        {
            if (percent < MinPercent) return MinPercent;
            if (percent > MaxPercent) return MaxPercent;
            return percent;
        }

        /// <summary>
        /// The multiplier applied to the compass's base (61px clock-sized) dimensions
        /// for a given size percentage.
        /// </summary>
        public static float UiScaleFor(int percent)
        {
            return ScaleAtDefaultPercent * ClampPercent(percent) / 100f;
        }
    }
}
