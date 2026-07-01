using Avalonia;
using Avalonia.Input;
using NormalCAD.Core.Geometry;

namespace NormalCAD.Controller.Commands
{
    public class ToggleThemeCommand : ICadCommand
    {
        public string Name => "_.THEME";
        public string LocalName => "THEME";
        public string Alias => "TEMA,TH";
        public bool IsInternal => false;

        public void Activate(CadController controller)
        {
            var isLight = !controller.IsLightTheme;
            controller.IsLightTheme = isLight;

            Application.Current!.RequestedThemeVariant = isLight ? Avalonia.Styling.ThemeVariant.Light : Avalonia.Styling.ThemeVariant.Dark;

            controller.Viewport.IsLightTheme = isLight;
            controller.Viewport.InvalidateVisual();

            controller.InputManager.SetPromptMessage($"Theme changed to {(isLight ? "Light" : "Dark")}.");

            controller.SetCommand(new BaseCommand());
        }

        public void Deactivate() { }
        public void OnPointerPressed(Point3d worldPt, PointerPressedEventArgs e) { }
        public void OnPointerMoved(Point3d worldPt) { }
        public void OnKeyDown(KeyEventArgs e) { }
    }
}
