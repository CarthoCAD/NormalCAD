using System.Threading.Tasks;
using NormalCAD.Core.Geometry;
using NormalCAD.Resources;

namespace NormalCAD.Controller.Commands
{
    public class SaveAsCommand : ICadCommand
    {
        public string Name => "_.SAVEAS";
        public string LocalName => CommandResources.Get("SAVEAS.LOCALNAME");
        public CommandType Type => CommandType.Immediate;
        public CommandFlags Flags => CommandFlags.None;
        public string Alias => "";

        public async Task ActivateAsync(CadController controller)
        {
            await SaveCommand.ShowSaveDialog(controller);
            controller.SetCommand(new BaseCommand());
        }

        public void Deactivate() { }
    }
}
