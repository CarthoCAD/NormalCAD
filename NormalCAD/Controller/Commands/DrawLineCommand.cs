using Avalonia.Input;
using NormalCAD.Core.Geometry;
using NormalCAD.Core.DatabaseServices;
using NormalCAD.Resources;
using NormalCAD.View.Controls;

namespace NormalCAD.Controller.Commands
{
    public class DrawLineCommand : ICadCommand
    {
        private CadController? _controller;
        private Point3d? _startPoint;

        public string Name => "_.LINE";
        public string LocalName => CommandResources.Get("LINE.LOCALNAME");
        public string Alias => CommandResources.Get("LINE.ALIAS");
        public bool IsInternal => false;

        public void Activate(CadController controller)
        {
            _controller = controller;
            _controller.Viewport.CurrentCursorState = CadCursorState.Crosshair;
            _startPoint = null;
        }

        public void Deactivate()
        {
            if (_controller != null)
            {
                _controller.Viewport.ActiveCommandPreview = null;
                _controller.Viewport.CurrentCursorState = CadCursorState.PickCross;
            }
        }

        public void OnPointerPressed(Point3d worldPt, PointerPressedEventArgs e)
        {
            if (_controller == null) return;

            if (!_startPoint.HasValue)
            {
                _startPoint = worldPt;
            }
            else
            {
                var line = new Line(_startPoint.Value, worldPt)
                {
                    Layer = _controller.ActiveLayer,
                    Color = _controller.ActiveColor
                };
                _controller.AddNewEntityToActiveSpace(line);

                _startPoint = worldPt;
            }
        }

        public void OnPointerMoved(Point3d worldPt)
        {
            if (_controller == null || !_startPoint.HasValue) return;

            _controller.Viewport.ActiveCommandPreview = new Line(_startPoint.Value, worldPt)
            {
                Layer = _controller.ActiveLayer,
                Color = _controller.ActiveColor
            };
        }

        public void OnKeyDown(KeyEventArgs e)
        {
            if (_controller == null) return;

            if (e.Key == Key.Enter || e.Key == Key.Space)
            {
                _controller.SetCommand(new BaseCommand());
                e.Handled = true;
            }
        }
    }
}
