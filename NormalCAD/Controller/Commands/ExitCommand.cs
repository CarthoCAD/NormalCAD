using Avalonia;
using Avalonia.Input;
using NormalCAD.Core.Geometry;

namespace NormalCAD.Controller.Commands
{
    public class ExitCommand : ICadCommand
    {
        public string Name => "_.QUIT";
        public string LocalName => "QUIT";
        public string Alias => "EXIT,Q";
        public bool IsInternal => false;

        public void Activate(CadController controller)
        {
            var window = (Application.Current?.ApplicationLifetime as Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime)?.MainWindow;
            window?.Close();
        }

        public void Deactivate() { }
        public void OnPointerPressed(Point3d worldPt, PointerPressedEventArgs e) { }
        public void OnPointerMoved(Point3d worldPt) { }
        public void OnKeyDown(KeyEventArgs e) { }
    }
}
