using Avalonia.Input;
using NormalCAD.Core.Geometry;
using NormalCAD.Resources;

namespace NormalCAD.Controller.Commands
{
    public class ToggleLanguageCommand : ICadCommand
    {
        private static string MsgChanged => CommandResources.Get("LANGUAGE.MSG.CHANGED");

        public string Name => "_.LANGUAGE";
        public string LocalName => CommandResources.Get("LANGUAGE.LOCALNAME");
        public string Alias => CommandResources.Get("LANGUAGE.ALIAS");
        public bool IsInternal => false;

        public void Activate(CadController controller)
        {
            var current = Services.LanguageService.CurrentCulture.Name;
            var next = current == "pt-BR" ? "" : "pt-BR";

            Services.LanguageService.SetCulture(next);

            var label = Services.LanguageService.GetDisplayLabel();
            controller.InputManager.SetPromptMessage(string.Format(MsgChanged, label));

            controller.SetCommand(new BaseCommand());
        }

        public void Deactivate() { }
        public void OnPointerPressed(Point3d worldPt, PointerPressedEventArgs e) { }
        public void OnPointerMoved(Point3d worldPt) { }
        public void OnKeyDown(KeyEventArgs e) { }
    }
}
