using Timberborn.CameraSystem;
using Timberborn.SingletonSystem;
using Timberborn.UILayoutSystem;
using UnityEngine;
using UnityEngine.UIElements;

namespace SimpleCompass
{
    /// <summary>
    /// A floating HUD compass, styled to match the game's own UI: a solid dark dial
    /// with a single gold ring border (the theme's accent color, e.g. CoreStyle.uss's
    /// .key-binding), a two-tone red/white needle bar, and the same white/bold text
    /// convention (CoreStyle.uss's .text--default/.text--bold) for its cardinal
    /// letters - see the modding kit at Timberborn_Data/StreamingAssets/Modding/UI.zip
    /// for the source USS this was read from. The game's own panel theme also applies
    /// to elements added here, so the font itself is inherited for free without
    /// setting it explicitly (1.0 set it in CoreStyle.uss's `*` rule; 1.1 sets it in
    /// the panel theme instead - either way it is not something this widget owns).
    ///
    /// The needle always points to true north on the map (world +Z, which matches
    /// the grid's Y axis - see Timberborn.Coordinates.CoordinateSystem), regardless
    /// of how the camera is panned, zoomed or rotated. Left-click-drag anywhere on
    /// the dial to move it; double-click toggles pinning it in place (shown by the
    /// ring going solid gold vs. dim white). Both the position and the pinned state
    /// persist across sessions via PlayerPrefs.
    /// </summary>
    public class SimpleCompassWidget : ILoadableSingleton, IUpdatableSingleton
    {
        private const string LeftPrefKey = "SimpleCompass.Left";
        private const string TopPrefKey = "SimpleCompass.Top";
        private const string PinnedPrefKey = "SimpleCompass.Pinned";

        // Based on the real HUD clock's dial size (GameMiscStyle.uss .clock-panel is
        // 61px), scaled up 1.5x.
        private const float UiScale = 1.5f;
        private const float DialDiameter = 61f * UiScale;
        private const float LabelInset = 2f * UiScale;
        private const float LabelFontSize = 11f * UiScale;
        private const float DefaultMargin = 16f;

        // Needle bar width, and length as a fraction of the dial's diameter so the tip
        // stops a couple of pixels short of the N/S/E/W letters instead of reaching
        // under them - expressed as a fraction (not a fixed pixel length) so it stays
        // correctly proportioned if UiScale/DialDiameter ever change.
        private const float NeedleWidth = 4f * UiScale;
        private const float NeedleLength = DialDiameter * 0.42f;

        // Matches CoreStyle.uss's .text--default white and the theme's gold accent
        // (used for e.g. .key-binding / .tooltip-key-binding backgrounds).
        private static readonly Color WhiteText = new Color(1f, 1f, 1f);
        private static readonly Color ThemeGold = new Color(154f / 255f, 134f / 255f, 94f / 255f);
        private static readonly Color RingColorUnpinned = new Color(1f, 1f, 1f, 0.6f);
        private static readonly Color NorthColor = new Color(0.92f, 0.35f, 0.28f);
        private static readonly Color SouthColor = new Color(0.88f, 0.86f, 0.82f);

        // The game's standard panel green - sampled directly from the flat center of
        // UI/Images/Backgrounds/bg-square-1 (the sprite behind dropdowns etc.), and
        // confirmed by the same rgb(21, 39, 34) value used as a flat CSS background
        // elsewhere in the theme (BatchControlStyle.uss).
        private static readonly Color DialBackground = new Color(21f / 255f, 39f / 255f, 34f / 255f, 1f);

        private readonly UILayout _uiLayout;
        private readonly CameraService _cameraService;

        private VisualElement _root;
        private VisualElement _needle;

        private bool _pinned;
        private bool _dragging;
        private Vector2 _dragOffset;

        public SimpleCompassWidget(UILayout uiLayout, CameraService cameraService)
        {
            _uiLayout = uiLayout;
            _cameraService = cameraService;
        }

        public void Load()
        {
            _root = BuildDial();
            _uiLayout.AddAbsoluteItem(_root);

            _pinned = PlayerPrefs.GetInt(PinnedPrefKey, 0) != 0;
            // Deliberately not derived from Screen.width/height here: those are raw
            // display pixels and can be on a different scale than the UI panel's own
            // coordinate space (resolvedStyle), which is what style.left/top use. A
            // small fixed on-screen margin avoids that mismatch entirely; the user can
            // drag it wherever they like from there, and that position gets saved.
            // Routed through ApplyClampedPosition (rather than assigned directly) so a
            // position saved at a larger resolution can't land off-screen after
            // switching to a smaller one.
            float savedLeft = PlayerPrefs.GetFloat(LeftPrefKey, DefaultMargin);
            float savedTop = PlayerPrefs.GetFloat(TopPrefKey, DefaultMargin);
            ApplyClampedPosition(savedLeft, savedTop);
            RefreshPinVisual();

            _root.RegisterCallback<MouseDownEvent>(OnRootMouseDown);
            _root.RegisterCallback<MouseMoveEvent>(OnRootMouseMove);
            _root.RegisterCallback<MouseUpEvent>(OnRootMouseUp);
            _root.RegisterCallback<MouseCaptureOutEvent>(_ => _dragging = false);

            // Re-clamp whenever the available screen area changes (window resize,
            // resolution change, UI scale change) so the widget can never end up
            // stranded outside the visible area.
            _root.parent.RegisterCallback<GeometryChangedEvent>(_ =>
                ApplyClampedPosition(_root.resolvedStyle.left, _root.resolvedStyle.top));
        }

        public void UpdateSingleton()
        {
            UpdateNeedleHeading();
        }

        private void UpdateNeedleHeading()
        {
            Transform cameraTransform = _cameraService.Transform;
            if (cameraTransform == null)
            {
                return;
            }

            // Project world-space true north onto the camera's screen-space right/up
            // axes to get the needle's on-screen bearing, so this stays correct however
            // the camera is oriented.
            Vector3 localNorth = cameraTransform.InverseTransformDirection(Vector3.forward);
            float rotationDegrees = CompassGeometry.NeedleRotationDegrees(localNorth.x, localNorth.y);
            _needle.style.rotate = new Rotate(Angle.Degrees(rotationDegrees));
        }

        // --- Dragging & pinning ---------------------------------------------------

        private void OnRootMouseDown(MouseDownEvent evt)
        {
            if (evt.button != 0)
            {
                return;
            }

            // clickCount is UI Toolkit's native double-click detection (matches the
            // OS's own double-click timing), so no hand-rolled timer is needed. Staying
            // on the left button and consuming it entirely here (CaptureMouse below,
            // StopPropagation everywhere) means this never touches the game's raw-polled
            // right-mouse camera rotation input at all.
            if (evt.clickCount >= 2)
            {
                if (_dragging)
                {
                    _dragging = false;
                    _root.ReleaseMouse();
                }

                TogglePinned();
                evt.StopPropagation();
                return;
            }

            if (_pinned)
            {
                return;
            }

            _dragging = true;
            _dragOffset = new Vector2(evt.mousePosition.x - _root.resolvedStyle.left, evt.mousePosition.y - _root.resolvedStyle.top);
            _root.CaptureMouse();
            evt.StopPropagation();
        }

        private void OnRootMouseMove(MouseMoveEvent evt)
        {
            if (!_dragging)
            {
                return;
            }

            float left = evt.mousePosition.x - _dragOffset.x;
            float top = evt.mousePosition.y - _dragOffset.y;
            ApplyClampedPosition(left, top);
        }

        private void OnRootMouseUp(MouseUpEvent evt)
        {
            if (!_dragging || evt.button != 0)
            {
                return;
            }

            _dragging = false;
            _root.ReleaseMouse();
            PlayerPrefs.SetFloat(LeftPrefKey, _root.resolvedStyle.left);
            PlayerPrefs.SetFloat(TopPrefKey, _root.resolvedStyle.top);
            PlayerPrefs.Save();
        }

        private void ApplyClampedPosition(float left, float top)
        {
            VisualElement bounds = _root.parent;
            float boundsWidth = bounds != null && bounds.resolvedStyle.width > 0f ? bounds.resolvedStyle.width : Screen.width;
            float boundsHeight = bounds != null && bounds.resolvedStyle.height > 0f ? bounds.resolvedStyle.height : Screen.height;

            (float clampedLeft, float clampedTop) = CompassGeometry.ClampPosition(left, top, boundsWidth, boundsHeight, DialDiameter);
            _root.style.left = clampedLeft;
            _root.style.top = clampedTop;
        }

        private void TogglePinned()
        {
            _pinned = !_pinned;
            PlayerPrefs.SetInt(PinnedPrefKey, _pinned ? 1 : 0);
            PlayerPrefs.Save();
            RefreshPinVisual();
        }

        private void RefreshPinVisual()
        {
            // The ring itself is the pin indicator: solid gold while pinned (locked in
            // place), a dimmer white while unpinned (draggable) - double-click anywhere
            // on the dial toggles between the two.
            SetUniformBorderColor(_root, _pinned ? ThemeGold : RingColorUnpinned);
        }

        // --- Visual tree construction -----------------------------------------------

        private VisualElement BuildDial()
        {
            var dial = new VisualElement { name = "SimpleCompassDial" };
            dial.style.position = Position.Absolute;
            dial.style.width = DialDiameter;
            dial.style.height = DialDiameter;
            dial.style.alignItems = Align.Center;
            dial.style.justifyContent = Justify.Center;
            dial.style.backgroundColor = DialBackground;
            SetUniformBorderRadius(dial, DialDiameter / 2f);
            SetUniformBorderWidth(dial, 3f);

            // Needle added before the labels so the labels always paint on top of it,
            // even if the geometry below ever falls short in an edge case.
            _needle = BuildNeedle();
            dial.Add(_needle);

            dial.Add(BuildCardinalLabel("N", TextAnchor.UpperCenter, bold: true));
            dial.Add(BuildCardinalLabel("S", TextAnchor.LowerCenter, bold: false));
            dial.Add(BuildCardinalLabel("E", TextAnchor.MiddleRight, bold: false));
            dial.Add(BuildCardinalLabel("W", TextAnchor.MiddleLeft, bold: false));

            return dial;
        }

        private static VisualElement BuildCardinalLabel(string text, TextAnchor anchor, bool bold)
        {
            var label = new Label(text) { pickingMode = PickingMode.Ignore };
            label.style.position = Position.Absolute;
            label.style.left = LabelInset;
            label.style.right = LabelInset;
            label.style.top = LabelInset;
            label.style.bottom = LabelInset;
            label.style.unityTextAlign = anchor;
            // Matches CoreStyle.uss .text--default; the font family itself is inherited
            // from the panel's own global stylesheet, so it's not set here.
            label.style.color = WhiteText;
            label.style.fontSize = LabelFontSize;
            label.style.unityFontStyleAndWeight = bold ? FontStyle.Bold : FontStyle.Normal;
            return label;
        }

        private static VisualElement BuildNeedle()
        {
            var needle = new VisualElement { name = "SimpleCompassNeedle", pickingMode = PickingMode.Ignore };
            needle.style.width = NeedleWidth;
            needle.style.height = NeedleLength;
            needle.style.transformOrigin = new TransformOrigin(Length.Percent(50), Length.Percent(50), 0);

            var north = new VisualElement { pickingMode = PickingMode.Ignore };
            north.style.width = NeedleWidth;
            north.style.height = NeedleLength / 2f;
            north.style.backgroundColor = NorthColor;
            north.style.borderTopLeftRadius = NeedleWidth / 2f;
            north.style.borderTopRightRadius = NeedleWidth / 2f;

            var south = new VisualElement { pickingMode = PickingMode.Ignore };
            south.style.width = NeedleWidth;
            south.style.height = NeedleLength / 2f;
            south.style.backgroundColor = SouthColor;
            south.style.borderBottomLeftRadius = NeedleWidth / 2f;
            south.style.borderBottomRightRadius = NeedleWidth / 2f;

            needle.Add(north);
            needle.Add(south);
            return needle;
        }

        private static void SetUniformBorderWidth(VisualElement element, float width)
        {
            element.style.borderLeftWidth = width;
            element.style.borderRightWidth = width;
            element.style.borderTopWidth = width;
            element.style.borderBottomWidth = width;
        }

        private static void SetUniformBorderColor(VisualElement element, Color color)
        {
            element.style.borderLeftColor = color;
            element.style.borderRightColor = color;
            element.style.borderTopColor = color;
            element.style.borderBottomColor = color;
        }

        private static void SetUniformBorderRadius(VisualElement element, float radius)
        {
            element.style.borderTopLeftRadius = radius;
            element.style.borderTopRightRadius = radius;
            element.style.borderBottomLeftRadius = radius;
            element.style.borderBottomRightRadius = radius;
        }
    }
}
