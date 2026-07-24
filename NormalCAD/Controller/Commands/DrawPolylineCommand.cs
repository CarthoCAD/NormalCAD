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
        private static string KeyArc => CommandResources.Get("PLINE.KEY.ARC");
        private static string KeyWidth => CommandResources.Get("PLINE.KEY.WIDTH");
        private static string KeyHalfwidth => CommandResources.Get("PLINE.KEY.HALFWIDTH");
        private static string KeyLength => CommandResources.Get("PLINE.KEY.LENGTH");
        private static string MsgNotImpl => CommandResources.Get("CMD.MSG.NOTIMPL");
        private static string PromptStartWidth => CommandResources.Get("PLINE.PROMPT.STARTWIDTH");
        private static string PromptEndWidth => CommandResources.Get("PLINE.PROMPT.ENDWIDTH");
        private static string PromptHalfWidth => CommandResources.Get("PLINE.PROMPT.HALFWIDTH");
        private static string PromptLength => CommandResources.Get("PLINE.PROMPT.LENGTH");

        private CadController? _controller;
        private Polyline _polyline = new();
        private Point3d? _lastCommittedPoint;
        private double _startWidth;
        private double _endWidth;

        public string Name => "_.PLINE";
        public string LocalName => CommandResources.Get("PLINE.LOCALNAME");
        public CommandType Type => CommandType.Interactive;
        public CommandFlags Flags => CommandFlags.None;
        public string Alias => CommandResources.Get("PLINE.ALIAS");

        private int CommittedCount => _polyline.NumberOfVertices;

        public Task ActivateAsync(CadController controller)
        {
            _controller = controller;
            _controller.Viewport.CurrentCursorState = CadCursorState.Crosshair;
            _polyline = new Polyline
            {
                Layer = _controller.ActiveLayer,
                Color = _controller.ActiveColor
            };
            _lastCommittedPoint = null;
            _controller.InputManager.SetPreview("polyline", _polyline);
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
            _polyline.AddVertexAt(0, Point2d.FromPoint3d(result.Value), 0.0, _startWidth, _endWidth);
            _lastCommittedPoint = result.Value;

            RegisterNextPointPrompt();
        }

        private void RegisterNextPointPrompt()
        {
            var keywords = CommittedCount >= 2
                ? new[] { KeyArc, KeyWidth, KeyHalfwidth, KeyLength, KeyUndo, KeyClose }
                : new[] { KeyArc, KeyWidth, KeyHalfwidth, KeyLength, KeyUndo };

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
                if (result.StringResult == KeyArc)
                {
                    _controller!.InputManager.SetPromptMessage(MsgNotImpl);
                    RegisterNextPointPrompt();
                    return;
                }
                if (result.StringResult == KeyWidth)
                {
                    RegisterWidthStart();
                    return;
                }
                if (result.StringResult == KeyHalfwidth)
                {
                    RegisterHalfwidth();
                    return;
                }
                if (result.StringResult == KeyLength)
                {
                    _controller!.InputManager.SetPromptMessage(MsgNotImpl);
                    RegisterNextPointPrompt();
                    return;
                }
                if (result.StringResult == KeyClose && CommittedCount >= 2)
                {
                    Finish(closed: true);
                    return;
                }
                if (result.StringResult == KeyUndo && _polyline.NumberOfVertices >= 2)
                {
                    _polyline.RemoveVertexAt(_polyline.NumberOfVertices - 1);
                    _lastCommittedPoint = _polyline.GetPoint3dAt(_polyline.NumberOfVertices - 1);
                    RegisterNextPointPrompt();
                    return;
                }
            }

            if (result.Status != PromptStatus.OK) { Finish(closed: false); return; }

            var worldPt = result.Value;
            _polyline.AddVertexAt(_polyline.NumberOfVertices,
                Point2d.FromPoint3d(worldPt), 0.0, _startWidth, _endWidth);
            _lastCommittedPoint = worldPt;

            RegisterNextPointPrompt();
        }

        private void RegisterWidthStart()
        {
            _controller!.InputManager.RegisterGetDistance(
                new PromptDistanceOptions
                {
                    Message = PromptStartWidth,
                    BasePoint = _lastCommittedPoint
                },
                w =>
                {
                    if (w.Status != PromptStatus.OK)
                    {
                        RegisterNextPointPrompt();
                        return;
                    }
                    _startWidth = w.Value;
                    _controller.InputManager.RegisterGetDistance(
                        new PromptDistanceOptions
                        {
                            Message = PromptEndWidth,
                            BasePoint = _lastCommittedPoint
                        },
                        w2 =>
                        {
                            if (w2.Status == PromptStatus.OK)
                                _endWidth = w2.Value;
                            RegisterNextPointPrompt();
                        });
                });
        }

        private void RegisterHalfwidth()
        {
            _controller!.InputManager.RegisterGetDistance(
                new PromptDistanceOptions
                {
                    Message = PromptHalfWidth,
                    BasePoint = _lastCommittedPoint
                },
                w =>
                {
                    if (w.Status == PromptStatus.OK)
                    {
                        _startWidth = w.Value * 2;
                        _endWidth = w.Value * 2;
                    }
                    RegisterNextPointPrompt();
                });
        }

        private void Finish(bool closed)
        {
            if (_controller == null || CommittedCount < 2)
            {
                _controller!.FinishCommand();
                return;
            }

            _polyline.Closed = closed;
            _polyline.Layer = _controller.ActiveLayer;
            _polyline.Color = _controller.ActiveColor;
            CadCoreHelper.AddNewEntityToCurrentSpace(_polyline);
            _controller.FinishCommand();
        }
    }
}
