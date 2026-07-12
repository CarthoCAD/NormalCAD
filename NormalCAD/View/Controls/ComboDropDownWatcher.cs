using Avalonia;
using Avalonia.Controls;
using Avalonia.VisualTree;

namespace NormalCAD.View.Controls
{
    // Forwards a ComboBox's drop-down open/close (a plain CLR event, not routed)
    // to the owning PropertyPalette so the sidebar auto-hide can be suppressed
    // while a dropdown is open. Applied per template via ComboDropDownWatcher.Enabled.
    public static class ComboDropDownWatcher
    {
        public static readonly AttachedProperty<bool> EnabledProperty =
            AvaloniaProperty.RegisterAttached<ComboBox, bool>("Enabled", typeof(ComboDropDownWatcher));

        public static void SetEnabled(ComboBox element, bool value) => element.SetValue(EnabledProperty, value);
        public static bool GetEnabled(ComboBox element) => element.GetValue(EnabledProperty);

        static ComboDropDownWatcher()
        {
            EnabledProperty.Changed.AddClassHandler<ComboBox>((combo, e) =>
            {
                if (!e.GetNewValue<bool>()) return;
                combo.DropDownOpened += (_, _) => Report(combo, true);
                combo.DropDownClosed += (_, _) => Report(combo, false);
            });
        }

        private static void Report(ComboBox combo, bool open)
            => combo.FindAncestorOfType<PropertyPalette>()?.SetDropDownOpen(open);
    }
}
