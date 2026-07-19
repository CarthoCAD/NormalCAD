using System.Threading.Tasks;
using NormalCAD.Core.DatabaseServices;
using NormalCAD.Core.EditorInput;
using NormalCAD.Core.Geometry;
using NormalCAD.Resources;
using NormalCAD.Utilities;
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
        private Point3d? _lastCommittedPoint;

        public string Name => "_.PLINE";
        public string LocalName => CommandResources.Get("PLINE.LOCALNAME");
        public CommandType Type => CommandType.Interactive;
        public CommandFlags Flags => CommandFlags.None;
        public string Alias => CommandResources.Get("PLINE.ALIAS");

        private int CommittedCount =>
            _polyline.NumberOfVertices > 0 ? _polyline.NumberOfVertices - 1 : 0;

        public Task ActivateAsync(CadController controller)
        {
            _controller = controller;
            _controller.Viewport.CurrentCursorState = CadCursorState.Crosshair;
            _polyline = new Polyline();
            _lastCommittedPoint = null;
            _controller.InputManager.SetPreview("polyline", _polyline);
            _controller.InputManager.RegisterMouseMove(OnMouseMove);
            RegisterFirstPointPrompt();
            return Task.CompletedTask;
        }

        public void Deactivate()
        {
            if (_controller != null)
            {
                _controller.InputManager.ClearAllRegistrations();
                _controller.Viewport.CurrentCursorState = CadCursorState.PickCross;
            }
        }

        private void RegisterFirstPointPrompt()
        {
            _controller!.InputManager.RegisterGetPoint(
                new PromptPointOptions { Message = PromptFirstPoint },
                OnFirstPoint);
        }

        private void OnFirstPoint(PromptPointResult result)
        {
            if (result.Status != PromptStatus.OK) { Finish(closed: false); return; }

            _polyline.Elevation = result.Value.Z;
            _polyline.AddVertexAt(0, Point2d.FromPoint3d(result.Value), 0.0, 0.0, 0.0);
            _polyline.AddVertexAt(1, Point2d.FromPoint3d(result.Value), 0.0, 0.0, 0.0);
            _lastCommittedPoint = result.Value;

            RegisterNextPointPrompt();
        }

        private void RegisterNextPointPrompt()
        {
            var keywords = CommittedCount >= 2
                ? new[] { KeyUndo, KeyClose }
                : new[] { KeyUndo };

            _controller!.InputManager.RegisterGetPoint(
                new PromptPointOptions
                {
                    Message = PromptNextPoint,
                    Keywords = keywords,
                    BasePoint = _lastCommittedPoint
                },
                OnNextPoint);
        }

        private void OnNextPoint(PromptPointResult result)
        {
            if (result.Status == PromptStatus.Keyword)
            {
                if (result.StringResult == KeyClose && CommittedCount >= 2)
                {
                    Finish(closed: true);
                    return;
                }
                if (result.StringResult == KeyUndo && _polyline.NumberOfVertices >= 2)
                {
                    _polyline.RemoveVertexAt(_polyline.NumberOfVertices - 2);
                    _lastCommittedPoint = _polyline.NumberOfVertices >= 2
                        ? _polyline.GetPoint3dAt(_polyline.NumberOfVertices - 2)
                        : (Point3d?)null;
                    _controller!.Viewport.InvalidateVisual();
                    if (_polyline.NumberOfVertices == 0)
                        RegisterFirstPointPrompt();
                    else
                        RegisterNextPointPrompt();
                    return;
                }
            }

            if (result.Status != PromptStatus.OK) { Finish(closed: false); return; }

            var worldPt = result.Value;
            _polyline.SetPointAt(_polyline.NumberOfVertices - 1,
                Point2d.FromPoint3d(worldPt));
            _polyline.AddVertexAt(_polyline.NumberOfVertices,
                Point2d.FromPoint3d(worldPt), 0.0, 0.0, 0.0);
            _lastCommittedPoint = worldPt;

            RegisterNextPointPrompt();
        }

        private void OnMouseMove(Point3d worldPt)
        {
            if (_controller == null || _polyline.NumberOfVertices == 0) return;

            _polyline.SetPointAt(_polyline.NumberOfVertices - 1,
                Point2d.FromPoint3d(worldPt));
            _controller.Viewport.InvalidateVisual();
        }

        private void Finish(bool closed)
        {
            if (_controller == null || CommittedCount < 2)
            {
                _controller!.SetCommand(new BaseCommand());
                return;
            }

            _polyline.RemoveVertexAt(_polyline.NumberOfVertices - 1);
            _polyline.Closed = closed;
            _polyline.Layer = _controller.ActiveLayer;
            _polyline.Color = _controller.ActiveColor;
            CadCoreHelper.AddNewEntityToCurrentSpace(_polyline);
            _controller.SetCommand(new BaseCommand());
        }
    }
}
