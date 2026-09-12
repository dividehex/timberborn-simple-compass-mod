using Timberborn.SettingsSystemUI;
using Timberborn.SingletonSystem;
using UnityEngine;
using UnityEngine.UIElements;

namespace SimpleCompass
{
    /// <summary>
    /// Adds a "Compass size" slider to the game's own Settings dialog, directly under
    /// its "UI scale factor" slider in the Interface section.
    ///
    /// The game offers no extension point for this: its SettingsBox loads a fixed
    /// Options/SettingsBox.uxml template. It is, however, bound under the public
    /// ISettingsController interface in the same contexts this mod runs in, and
    /// GetPanel() hands back the box's root, so the row is inserted into that tree
    /// here. Running as an IPostLoadableSingleton guarantees the box has already been
    /// built. The row reuses the template's own style classes (settings-element,
    /// settings-slider, settings-slider__slider, settings-slider__end-label - see
    /// OptionsStyle.uss in the modding kit's UI.zip) so it is styled identically to the
    /// game's sliders. Because this depends on the template's element names staying
    /// as they are, a missing element is logged and the slider simply skipped; the
    /// compass itself is unaffected.
    /// </summary>
    public class CompassSizeSettingsSlider : IPostLoadableSingleton
    {
        private const string ContentName = "Content";
        private const string UiScaleRowName = "UIScaleFactor";
        private const string RowName = "SimpleCompassSize";
        private const string SliderLabel = "Compass size";

        private readonly ISettingsController _settingsController;
        private readonly CompassScaleSetting _scaleSetting;

        public CompassSizeSettingsSlider(ISettingsController settingsController, CompassScaleSetting scaleSetting)
        {
            _settingsController = settingsController;
            _scaleSetting = scaleSetting;
        }

        public void PostLoad()
        {
            VisualElement root = _settingsController.GetPanel();
            VisualElement anchor = root?.Q<ScrollView>(ContentName)?.Q<VisualElement>(UiScaleRowName);
            if (anchor?.parent == null)
            {
                Debug.LogWarning($"[SimpleCompass] Settings dialog has no '{ContentName}'/'{UiScaleRowName}' element; the compass size slider was not added.");
                return;
            }

            anchor.parent.Insert(anchor.parent.IndexOf(anchor) + 1, BuildRow());
        }

        private VisualElement BuildRow()
        {
            var row = new VisualElement { name = RowName };
            row.AddToClassList("settings-element");
            row.AddToClassList("settings-slider");
            row.AddToClassList("settings-text");

            var slider = new SliderInt(SliderLabel, CompassScale.MinPercent, CompassScale.MaxPercent)
            {
                name = "Slider",
                focusable = false,
                value = _scaleSetting.Percent,
            };
            slider.AddToClassList("settings-slider__slider");

            var valueLabel = new Label(FormatPercent(_scaleSetting.Percent)) { name = "Value" };
            valueLabel.AddToClassList("settings-slider__end-label");

            slider.RegisterValueChangedCallback(evt =>
            {
                _scaleSetting.Percent = evt.newValue;
                valueLabel.text = FormatPercent(_scaleSetting.Percent);
            });

            row.Add(slider);
            row.Add(valueLabel);
            return row;
        }

        private static string FormatPercent(int percent)
        {
            return percent + "%";
        }
    }
}
