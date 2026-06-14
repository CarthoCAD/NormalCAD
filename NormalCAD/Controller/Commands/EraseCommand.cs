using System.Collections.Generic;
using Avalonia.Input;
using NormalCAD.Core;
using NormalCAD.Core.Geometry;
using NormalCAD.Core.Entities;

namespace NormalCAD.Controller.Commands
{
    public class EraseCommand : ICadCommand
    {
        public string Name => "Apagar";

        public void Activate(CadController controller)
        {
            var viewport = controller.Viewport;
            var selected = viewport.SelectedEntityIds;

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

                selected.Clear();
                controller.NotifySelectionChanged();
                controller.NotifyDatabaseChanged();
            }

            // Retorna imediatamente para a ferramenta de seleção (BaseCommand)
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
