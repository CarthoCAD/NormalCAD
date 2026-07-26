using System.Threading.Tasks;
using NormalCAD.Core.DatabaseServices;
using NormalCAD.Core.Geometry;
using NormalCAD.Resources;
using NormalCAD.Utilities;

namespace NormalCAD.Controller.Commands
{
    public class EraseCommand : ICadCommand
    {
        public string Name => "_.ERASE";
        public string LocalName => CommandResources.Get("ERASE.LOCALNAME");
        public CommandType Type => CommandType.Immediate;
        public CommandFlags Flags => CommandFlags.UsePickSet;
        public string Alias => CommandResources.Get("ERASE.ALIAS");

        public Task ActivateAsync()
        {
            var selected = CadController.Current.SelectedEntityIds;

            if (selected.Count > 0)
            {
                CadCoreHelper.EditCurrentSpace((trans, currentSpace) =>
                {
                    foreach (var entId in selected)
                    {
                        currentSpace.RemoveEntity(entId);
                    }
                });

                CadController.Current.ClearSelection();
            }

            CadController.Current.FinishCommand();
            return Task.CompletedTask;
        }

        public void Deactivate()
        {
        }
    }
}
