using System;
using UnityEngine;

namespace SimpleCompass
{
    /// <summary>
    /// The user's chosen compass size, persisted via PlayerPrefs alongside the
    /// widget's position and pinned state. The single source of truth shared by the
    /// settings slider (which writes it) and the widget (which listens to it).
    /// </summary>
    public class CompassScaleSetting
    {
        private const string PercentPrefKey = "SimpleCompass.SizePercent";

        private int _percent = CompassScale.ClampPercent(PlayerPrefs.GetInt(PercentPrefKey, CompassScale.DefaultPercent));

        /// <summary>Raised after <see cref="Percent"/> changes, with the new value.</summary>
        public event Action<int> Changed;

        public int Percent
        {
            get => _percent;
            set
            {
                int clamped = CompassScale.ClampPercent(value);
                if (clamped == _percent)
                {
                    return;
                }

                _percent = clamped;
                PlayerPrefs.SetInt(PercentPrefKey, clamped);
                PlayerPrefs.Save();
                Changed?.Invoke(clamped);
            }
        }

        public float UiScale => CompassScale.UiScaleFor(_percent);
    }
}
