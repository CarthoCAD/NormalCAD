using System.Collections.Generic;
using Avalonia.Input;
using NormalCAD.Core.DatabaseServices;
using NormalCAD.Core.Geometry;
using NormalCAD.Resources;

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
            using (var trans = controller.Database.TransactionManager.StartTransaction())
            {
                if (controller.Database.TryGetObject(controller.Database.BlockTableId, out var btObj) && btObj is BlockTable bt)
                {
                    var modelSpaceId = bt[BlockTableRecord.ModelSpace];
                    if (controller.Database.TryGetObject(modelSpaceId, out var btrObj) && btrObj is BlockTableRecord btr)
                    {
                        var ids = new List<ObjectId>(btr.GetEntityIds());
                        foreach (var id in ids)
                        {
                            btr.RemoveEntity(id);
                        }
                    }
                }
                trans.Commit();
            }

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
