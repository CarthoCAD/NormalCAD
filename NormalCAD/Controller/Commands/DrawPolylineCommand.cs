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
        private Polyline _polyline = new();

        public string Name => "_.PLINE";
        public string LocalName => CommandResources.Get("PLINE.LOCALNAME");
        public string Alias => CommandResources.Get("PLINE.ALIAS");
        public bool IsInternal => false;

        private int CommittedCount =>
            _polyline.NumberOfVertices > 0 ? _polyline.NumberOfVertices - 1 : 0;

        public void Activate(CadController controller)
        {
            _controller = controller;
            _controller.Viewport.CurrentCursorState = CadCursorState.Crosshair;
            _polyline = new Polyline();
            _controller.Viewport.ActiveCommandPreview = _polyline;

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

            if (CommittedCount == 0)
                _controller.InputManager.SetCurrentPrompt(name, PromptFirstPoint);
            else if (CommittedCount == 1)
                _controller.InputManager.SetCurrentPrompt(name, PromptNextPoint,
                    new[] { KeyUndo }, OnKeyword);
            else
                _controller.InputManager.SetCurrentPrompt(name, PromptNextPoint,
                    new[] { KeyUndo, KeyClose }, OnKeyword);
        }

        private void OnKeyword(string keyword)
        {
            if (_controller == null) return;

            if (keyword == KeyClose && CommittedCount >= 2)
            {
                CommitPolyline(closed: true);
            }
            else if (keyword == KeyUndo && _polyline.NumberOfVertices >= 2)
            {
                _polyline.RemoveVertexAt(_polyline.NumberOfVertices - 2);
                UpdatePrompt();
                _controller.Viewport.InvalidateVisual();
            }
        }

        public void OnPointerPressed(Point3d worldPt, PointerPressedEventArgs e)
        {
            if (_controller == null) return;

            var props = e.GetCurrentPoint(_controller.Viewport).Properties;
            if (props.IsRightButtonPressed && CommittedCount >= 2)
            {
                CommitPolyline(closed: false);
                return;
            }

            if (_polyline.NumberOfVertices == 0)
            {
                _polyline.Elevation = worldPt.Z;
                _polyline.AddVertexAt(0, Point2d.FromPoint3d(worldPt), 0.0, 0.0, 0.0);
            }
            else
            {
                _polyline.SetPointAt(_polyline.NumberOfVertices - 1, Point2d.FromPoint3d(worldPt));
            }

            _polyline.AddVertexAt(_polyline.NumberOfVertices, Point2d.FromPoint3d(worldPt), 0.0, 0.0, 0.0);

            UpdatePrompt();
        }

        public void OnPointerMoved(Point3d worldPt)
        {
            if (_controller == null || _polyline.NumberOfVertices == 0) return;

            _polyline.SetPointAt(_polyline.NumberOfVertices - 1, Point2d.FromPoint3d(worldPt));
            _controller.Viewport.InvalidateVisual();
        }

        public void OnKeyDown(KeyEventArgs e)
        {
            if (_controller == null) return;

            if ((e.Key == Key.Enter || e.Key == Key.Space) && CommittedCount >= 2)
            {
                CommitPolyline(closed: false);
                e.Handled = true;
            }
        }

        private void CommitPolyline(bool closed)
        {
            if (_controller == null || CommittedCount < 2) return;

            _polyline.RemoveVertexAt(_polyline.NumberOfVertices - 1);
            _polyline.Closed = closed;
            _polyline.Layer = _controller.ActiveLayer;
            _polyline.Color = _controller.ActiveColor;
            _controller.AddNewEntityToActiveSpace(_polyline);
            _controller.SetCommand(new BaseCommand());
        }
    }
}
