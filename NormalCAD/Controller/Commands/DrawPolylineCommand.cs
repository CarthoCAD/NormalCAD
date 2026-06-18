using System.Collections.Generic;
using Avalonia.Input;
using NormalCAD.Core.Geometry;
using NormalCAD.Core.Entities;
using NormalCAD.View.Controls;

namespace NormalCAD.Controller.Commands
{
    public class DrawPolylineCommand : ICadCommand
    {
        private CadController? _controller;
        private readonly List<Point3d> _vertices = new();

        public string Name => "_.PLINE";
        public string LocalName => "PLINE";
        public string Alias => "PL";
        public bool IsInternal => false;

        public void Activate(CadController controller)
        {
            _controller = controller;
            _controller.Viewport.CurrentCursorState = CadCursorState.Crosshair;
            _vertices.Clear();
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

            var props = e.GetCurrentPoint(_controller.Viewport).Properties;
            if (props.IsRightButtonPressed && _vertices.Count >= 2)
            {
                CommitPolyline();
                return;
            }

            _vertices.Add(worldPt);
        }

        public void OnPointerMoved(Point3d worldPt)
        {
            if (_controller == null || _vertices.Count == 0) return;

            var previewVerts = new List<Point3d>(_vertices) { worldPt };
            _controller.Viewport.ActiveCommandPreview = new LwPolyline(previewVerts, false);
        }

        public void OnKeyDown(KeyEventArgs e)
        {
            if (_controller == null) return;

            if (e.Key == Key.Enter && _vertices.Count >= 2)
            {
                CommitPolyline();
                e.Handled = true;
                return;
            }

            if (e.Key == Key.C && _vertices.Count >= 2)
            {
                var poly = new LwPolyline(_vertices, true)
                {
                    Layer = _controller.ActiveLayer,
                    Color = _controller.ActiveColor
                };
                _controller.AddNewEntityToActiveSpace(poly);
                _controller.SetCommand(new BaseCommand());
                e.Handled = true;
                return;
            }
        }

        private void CommitPolyline()
        {
            if (_controller == null || _vertices.Count < 2) return;

            var poly = new LwPolyline(_vertices, false)
            {
                Layer = _controller.ActiveLayer,
                Color = _controller.ActiveColor
            };
            _controller.AddNewEntityToActiveSpace(poly);
            _controller.SetCommand(new BaseCommand());
        }
    }
}
