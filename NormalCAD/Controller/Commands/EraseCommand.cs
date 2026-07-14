using Avalonia.Input;
using NormalCAD.Core.ApplicationServices;
using NormalCAD.Core.DatabaseServices;
using NormalCAD.Core.Geometry;
using NormalCAD.Resources;

namespace NormalCAD.Controller.Commands
{
    public class EraseCommand : ICadCommand
    {
        public string Name => "_.ERASE";
        public string LocalName => CommandResources.Get("ERASE.LOCALNAME");
        public string Alias => CommandResources.Get("ERASE.ALIAS");
        public bool IsInternal => false;

        public void Activate(CadController controller)
        {
            var doc = Application.DocumentManager.MdiActiveDocument;
            if (doc == null)
            {
                controller.SetCommand(new BaseCommand());
                return;
            }

            var db = doc.Database;
            var selected = controller.SelectedEntityIds;

            if (selected.Count > 0)
            {
                using (var trans = db.TransactionManager.StartTransaction())
                {
                    if (db.TryGetObject(db.BlockTableId, out var btObj) && btObj is BlockTable bt)
                    {
                        var modelSpaceId = bt[BlockTableRecord.ModelSpace];
                        if (db.TryGetObject(modelSpaceId, out var btrObj) && btrObj is BlockTableRecord btr)
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
