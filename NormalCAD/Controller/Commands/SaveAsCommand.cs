using Avalonia.Input;
using NormalCAD.Core.Geometry;

namespace NormalCAD.Controller.Commands
{
    public class SaveAsCommand : ICadCommand
    {
        public string Name => "_.SAVEAS";
        public string LocalName => "SAVEAS";
        public string Alias => "";
        public bool IsInternal => false;

        public async void Activate(CadController controller)
        {
            await SaveCommand.ShowSaveDialog(controller);
            controller.SetCommand(new BaseCommand());
        }

        public void Deactivate() { }
        public void OnPointerPressed(Point3d worldPt, PointerPressedEventArgs e) { }
        public void OnPointerMoved(Point3d worldPt) { }
        public void OnKeyDown(KeyEventArgs e) { }
    }
}
