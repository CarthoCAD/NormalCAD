using Avalonia.Input;
using NormalCAD.Core.Geometry;
using NormalCAD.Controller.Commands;

namespace NormalCAD.Controller
{
    public class InputManager
    {
        private readonly CadController _controller;

        public InputManager(CadController controller)
        {
            _controller = controller;
        }

        public void OnPointerPressed(Point3d worldPt, PointerPressedEventArgs e)
        {
            _controller.ActiveCommand?.OnPointerPressed(worldPt, e);
        }

        public void OnPointerMoved(Point3d worldPt)
        {
            _controller.ActiveCommand?.OnPointerMoved(worldPt);
        }

        public void OnKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                _controller.SetCommand(new EraseCommand());
                e.Handled = true;
                return;
            }

            if (e.Key == Key.Escape || e.Key == Key.Return || e.Key == Key.Space)
            {
                _controller.CancelCurrentCommand();
                e.Handled = true;
                return;
            }

            _controller.ActiveCommand?.OnKeyDown(e);
        }
    }
}
