using Avalonia.Input;
using NormalCAD.Core.DatabaseServices;
using NormalCAD.Core.EditorInput;
using NormalCAD.Core.Geometry;
using NormalCAD.Resources;
using NormalCAD.Utilities;
using NormalCAD.View.Controls;

namespace NormalCAD.Controller.Commands
{
    public class DrawLineCommand : ICadCommand
    {
        private static string PromptFirstPoint => CommandResources.Get("LINE.PROMPT.FIRSTPOINT");
        private static string PromptNextPoint => CommandResources.Get("LINE.PROMPT.NEXTPOINT");

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
            _controller.InputManager.RegisterMouseMove(OnMouseMove);
            RegisterFirstPointPrompt();
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
                OnPoint);
        }

        private void RegisterNextPointPrompt()
        {
            _controller!.InputManager.RegisterGetPoint(
                new PromptPointOptions
                {
                    Message = PromptNextPoint,
                    BasePoint = _startPoint
                },
                OnPoint);
        }

        private void OnPoint(PromptPointResult result)
        {
            if (result.Status != PromptStatus.OK) { Finish(); return; }

            var worldPt = result.Value;

            if (!_startPoint.HasValue)
            {
                _startPoint = worldPt;
                RegisterNextPointPrompt();
            }
            else
            {
                var line = new Line(_startPoint.Value, worldPt)
                {
                    Layer = _controller!.ActiveLayer,
                    Color = _controller.ActiveColor
                };
                CadCoreHelper.AddNewEntityToCurrentSpace(line);

                _startPoint = worldPt;
                RegisterNextPointPrompt();
            }
        }

        private void OnMouseMove(Point3d worldPt)
        {
            if (_controller == null || !_startPoint.HasValue) return;

            _controller.InputManager.SetPreview("line",
                new Line(_startPoint.Value, worldPt)
                {
                    Layer = _controller.ActiveLayer,
                    Color = _controller.ActiveColor
                });
            _controller.Viewport.InvalidateVisual();
        }

        private void Finish()
        {
            _controller!.SetCommand(new BaseCommand());
        }

        public void OnPointerPressed(Point3d worldPt, PointerPressedEventArgs e) { }
        public void OnPointerMoved(Point3d worldPt) { }
        public void OnKeyDown(KeyEventArgs e) { }
    }
}
