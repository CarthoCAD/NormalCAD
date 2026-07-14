using Avalonia.Input;
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
        public string Alias => CommandResources.Get("ERASE.ALIAS");
        public bool IsInternal => false;

        public void Activate(CadController controller)
        {
            var selected = controller.SelectedEntityIds;

            if (selected.Count > 0)
            {
                CadCoreHelper.EditCurrentSpace((trans, currentSpace) =>
                {
                    foreach (var entId in selected)
                    {
                        currentSpace.RemoveEntity(entId);
                    }
                });

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
