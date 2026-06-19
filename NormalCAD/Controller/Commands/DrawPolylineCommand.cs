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
        private readonly List<Point2d> _vertices = new();
        private double _elevation;

        public string Name => "_.PLINE";
        public string LocalName => "PLINE";
        public string Alias => "PL";
        public bool IsInternal => false;

        public void Activate(CadController controller)
        {
            _controller = controller;
            _controller.Viewport.CurrentCursorState = CadCursorState.Crosshair;
            _vertices.Clear();
            _elevation = 0;
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
                CommitPolyline(closed: false);
                return;
            }

            if (_vertices.Count == 0)
                _elevation = worldPt.Z;

            _vertices.Add(Point2d.FromPoint3d(worldPt));
        }

        public void OnPointerMoved(Point3d worldPt)
        {
            if (_controller == null || _vertices.Count == 0) return;

            var previewVerts = new List<Point2d>(_vertices) { Point2d.FromPoint3d(worldPt) };
            var preview = new Polyline(previewVerts, false) { Elevation = _elevation };
            _controller.Viewport.ActiveCommandPreview = preview;
        }

        public void OnKeyDown(KeyEventArgs e)
        {
            if (_controller == null) return;

            if ((e.Key == Key.Enter || e.Key == Key.Space) && _vertices.Count >= 2)
            {
                CommitPolyline(closed: false);
                e.Handled = true;
                return;
            }

            if (e.Key == Key.C && _vertices.Count >= 2)
            {
                CommitPolyline(closed: true);
                e.Handled = true;
            }
        }

        private void CommitPolyline(bool closed)
        {
            if (_controller == null || _vertices.Count < 2) return;

            var poly = new Polyline(_vertices, closed) { Elevation = _elevation };
            poly.Layer = _controller.ActiveLayer;
            poly.Color = _controller.ActiveColor;
            _controller.AddNewEntityToActiveSpace(poly);
            _controller.SetCommand(new BaseCommand());
        }
    }
}
