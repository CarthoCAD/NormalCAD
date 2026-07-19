using System.Collections.Generic;
using System.Threading.Tasks;
using NormalCAD.Core.DatabaseServices;
using NormalCAD.Core.Geometry;
using NormalCAD.Resources;
using NormalCAD.Utilities;

namespace NormalCAD.Controller.Commands
{
    public class CleanAllCommand : ICadCommand
    {
        private static string MsgCleared => CommandResources.Get("CLEANALL.MSG.CLEARED");

        public string Name => "_.CLEANALL";
        public string LocalName => CommandResources.Get("CLEANALL.LOCALNAME");
        public CommandType Type => CommandType.Immediate;
        public CommandFlags Flags => CommandFlags.None;
        public string Alias => CommandResources.Get("CLEANALL.ALIAS");

        public Task ActivateAsync(CadController controller)
        {
            CadCoreHelper.EditCurrentSpace((trans, currentSpace) =>
            {
                var ids = new List<ObjectId>(currentSpace.GetEntityIds());
                foreach (var id in ids)
                {
                    currentSpace.RemoveEntity(id);
                }
            });

            controller.ClearSelection();
            controller.InputManager.SetPromptMessage(MsgCleared);

            controller.SetCommand(new BaseCommand());
            return Task.CompletedTask;
        }

        public void Deactivate() { }
    }
}
