using System.Collections.Generic;
using Avalonia.Input;
using NormalCAD.Core.Geometry;
using NormalCAD.Core.DatabaseServices;
using NormalCAD.Resources;
using NormalCAD.View.Controls;

namespace NormalCAD.Controller.Commands
{
    public class DrawPolylineCommand : ICadCommand
    {
        private static string PromptFirstPoint => CommandResources.Get("PLINE.PROMPT.FIRSTPOINT");
        private static string PromptNextPoint => CommandResources.Get("PLINE.PROMPT.NEXTPOINT");
        private static string KeyUndo => CommandResources.Get("PLINE.KEY.UNDO");
        private static string KeyClose => CommandResources.Get("PLINE.KEY.CLOSE");

        private CadController? _controller;
        private readonly List<Point2d> _vertices = new();
        private double _elevation;
        private Point3d _lastWorldPoint;

        public string Name => "_.PLINE";
        public string LocalName => CommandResources.Get("PLINE.LOCALNAME");
        public string Alias => CommandResources.Get("PLINE.ALIAS");
        public bool IsInternal => false;

        public void Activate(CadController controller)
        {
            _controller = controller;
            _controller.Viewport.CurrentCursorState = CadCursorState.Crosshair;
            _vertices.Clear();
            _elevation = 0;

            UpdatePrompt();
        }

        public void Deactivate()
        {
            if (_controller != null)
            {
                _controller.InputManager.ClearKeywords();
                _controller.Viewport.ActiveCommandPreview = null;
                _controller.Viewport.CurrentCursorState = CadCursorState.PickCross;
            }
        }

        private void UpdatePrompt()
        {
            if (_controller == null) return;

            var name = LocalName;

            if (_vertices.Count == 0)
            {
                _controller.InputManager.SetCurrentPrompt(name, PromptFirstPoint);
            }
            else if (_vertices.Count == 1)
            {
                _controller.InputManager.SetCurrentPrompt(name, PromptNextPoint,
                    new[] { KeyUndo }, OnKeyword);
            }
            else
            {
                _controller.InputManager.SetCurrentPrompt(name, PromptNextPoint,
                    new[] { KeyUndo, KeyClose }, OnKeyword);
            }
        }

        private void OnKeyword(string keyword)
        {
            if (_controller == null) return;

            if (keyword == KeyClose && _vertices.Count >= 2)
            {
                CommitPolyline(closed: true);
            }
            else if (keyword == KeyUndo && _vertices.Count > 0)
            {
                _vertices.RemoveAt(_vertices.Count - 1);
                UpdatePrompt();

                if (_vertices.Count > 0)
                {
                    var previewVerts = new List<Point2d>(_vertices)
                        { Point2d.FromPoint3d(_lastWorldPoint) };
                    var preview = new Polyline(previewVerts, false) { Elevation = _elevation };
                    _controller.Viewport.ActiveCommandPreview = preview;
                }
                else
                {
                    _controller.Viewport.ActiveCommandPreview = null;
                }

                _controller.Viewport.InvalidateVisual();
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
            UpdatePrompt();
        }

        public void OnPointerMoved(Point3d worldPt)
        {
            _lastWorldPoint = worldPt;
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
