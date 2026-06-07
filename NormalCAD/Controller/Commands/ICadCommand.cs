using Avalonia.Input;
using NormalCAD.Core.Geometry;

namespace NormalCAD.Controller.Commands
{
    public interface ICadCommand
    {
        string Name { get; }
        void Activate(CadController controller);
        void Deactivate();
        void OnPointerPressed(Point3d worldPt, PointerPressedEventArgs e);
        void OnPointerMoved(Point3d worldPt);
        void OnKeyDown(KeyEventArgs e);
    }
}
