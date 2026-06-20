using Avalonia.Input;
using NormalCAD.Core.DatabaseServices;
using NormalCAD.Core.Geometry;

namespace NormalCAD.Controller.Commands
{
    public class EraseCommand : ICadCommand
    {
        public string Name => "_.ERASE";
        public string LocalName => "ERASE";
        public string Alias => "E";
        public bool IsInternal => false;

        public void Activate(CadController controller)
        {
            var selected = controller.SelectedEntityIds;

            if (selected.Count > 0)
            {
                using (var trans = controller.Database.TransactionManager.StartTransaction())
                {
                    if (controller.Database.TryGetObject(controller.Database.BlockTableId, out var btObj) && btObj is BlockTable bt)
                    {
                        var modelSpaceId = bt[BlockTableRecord.ModelSpace];
                        if (controller.Database.TryGetObject(modelSpaceId, out var btrObj) && btrObj is BlockTableRecord btr)
                        {
                            foreach (var entId in selected)
                            {
                                btr.RemoveEntity(entId);
                            }
                        }
                    }
                    trans.Commit();
                }

                controller.ClearSelection();
            }

            controller.SetCommand(new BaseCommand());
        }

        public void Deactivate()
        {
        }

        public void OnPointerPressed(Point3d worldPt, PointerPressedEventArgs e)
        {
        }

        public void OnPointerMoved(Point3d worldPt)
        {
        }

        public void OnKeyDown(KeyEventArgs e)
        {
        }
    }
}
