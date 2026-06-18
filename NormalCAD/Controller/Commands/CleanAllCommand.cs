using System.Collections.Generic;
using Avalonia.Input;
using NormalCAD.Core;
using NormalCAD.Core.Geometry;

namespace NormalCAD.Controller.Commands
{
    public class CleanAllCommand : ICadCommand
    {
        public string Name => "_.CLEANALL";
        public string LocalName => "CLEANALL";
        public string Alias => "CLA";
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
            controller.InputManager.SetPromptMessage("Desenho limpo com sucesso.");

            controller.SetCommand(new BaseCommand());
        }

        public void Deactivate() { }
        public void OnPointerPressed(Point3d worldPt, PointerPressedEventArgs e) { }
        public void OnPointerMoved(Point3d worldPt) { }
        public void OnKeyDown(KeyEventArgs e) { }
    }
}
