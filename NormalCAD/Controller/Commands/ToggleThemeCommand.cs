using Avalonia.Input;
using NormalCAD.Core.Geometry;
using NormalCAD.Resources;

namespace NormalCAD.Controller.Commands
{
    public class ToggleThemeCommand : ICadCommand
    {
        private static string MsgLight => CommandResources.Get("THEME.MSG.LIGHT");
        private static string MsgDark => CommandResources.Get("THEME.MSG.DARK");
        private static string MsgChanged => CommandResources.Get("THEME.MSG.CHANGED");

        public string Name => "_.THEME";
        public string LocalName => CommandResources.Get("THEME.LOCALNAME");
        public string Alias => CommandResources.Get("THEME.ALIAS");
        public bool IsInternal => false;

        public void Activate(CadController controller)
        {
            var isLight = !controller.IsLightTheme;
            controller.ApplyTheme(isLight);
            Services.ConfigService.Update(c => c.Theme = isLight ? "Light" : "Dark");

            var themeName = isLight ? MsgLight : MsgDark;
            controller.InputManager.SetPromptMessage(string.Format(MsgChanged, themeName));

            controller.SetCommand(new BaseCommand());
        }

        public void Deactivate() { }
        public void OnPointerPressed(Point3d worldPt, PointerPressedEventArgs e) { }
        public void OnPointerMoved(Point3d worldPt) { }
        public void OnKeyDown(KeyEventArgs e) { }
    }
}
