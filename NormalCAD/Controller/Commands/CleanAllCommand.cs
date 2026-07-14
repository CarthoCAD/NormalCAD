using System.Collections.Generic;
using Avalonia.Input;
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
        public string Alias => CommandResources.Get("CLEANALL.ALIAS");
        public bool IsInternal => false;

        public void Activate(CadController controller)
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
        }

        public void Deactivate() { }
        public void OnPointerPressed(Point3d worldPt, PointerPressedEventArgs e) { }
        public void OnPointerMoved(Point3d worldPt) { }
        public void OnKeyDown(KeyEventArgs e) { }
    }
}
