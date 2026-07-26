using System.Threading.Tasks;
using NormalCAD.Core.Geometry;
using NormalCAD.Resources;

namespace NormalCAD.Controller.Commands
{
    public class ToggleLanguageCommand : ICadCommand
    {
        private static string MsgChanged => CommandResources.Get("LANGUAGE.MSG.CHANGED");

        public string Name => "_.LANGUAGE";
        public string LocalName => CommandResources.Get("LANGUAGE.LOCALNAME");
        public CommandType Type => CommandType.Immediate;
        public CommandFlags Flags => CommandFlags.None;
        public string Alias => CommandResources.Get("LANGUAGE.ALIAS");

        public Task ActivateAsync()
        {
            var current = Services.LanguageService.CurrentCulture.Name;
            var next = current == "pt-BR" ? "" : "pt-BR";

            Services.LanguageService.SetCulture(next);

            var label = Services.LanguageService.GetDisplayLabel();
            CadController.Current.InputManager.SetPromptMessage(string.Format(MsgChanged, label));

            CadController.Current.FinishCommand();
            return Task.CompletedTask;
        }

        public void Deactivate() { }
    }
}
