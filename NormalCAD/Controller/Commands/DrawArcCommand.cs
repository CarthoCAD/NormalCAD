using System;
using Avalonia.Input;
using NormalCAD.Core.DatabaseServices;
using NormalCAD.Core.EditorInput;
using NormalCAD.Core.Geometry;
using NormalCAD.Resources;
using NormalCAD.Utilities;
using NormalCAD.View.Controls;

namespace NormalCAD.Controller.Commands
{
    public class DrawArcCommand : ICadCommand
    {
        private static string PromptCenterPoint => CommandResources.Get("ARC.PROMPT.CENTERPOINT");
        private static string PromptRadius => CommandResources.Get("ARC.PROMPT.RADIUS");
        private static string PromptStartAngle => CommandResources.Get("ARC.PROMPT.STARTANGLE");
        private static string PromptEndAngle => CommandResources.Get("ARC.PROMPT.ENDANGLE");

        private CadController? _controller;
        private Point3d? _center;
        private double _radius;
        private double _startAngle;

        public string Name => "_.ARC";
        public string LocalName => CommandResources.Get("ARC.LOCALNAME");
        public string Alias => CommandResources.Get("ARC.ALIAS");
        public bool IsInternal => false;

        public void Activate(CadController controller)
        {
            _controller = controller;
            _controller.Viewport.CurrentCursorState = CadCursorState.Crosshair;
            _center = null;
            _radius = 0;
            _controller.InputManager.RegisterMouseMove(OnMouseMove);
            RegisterCenterPrompt();
        }

        public void Deactivate()
        {
            if (_controller != null)
            {
                _controller.InputManager.ClearAllRegistrations();
                _controller.Viewport.CurrentCursorState = CadCursorState.PickCross;
            }
        }

        private void RegisterCenterPrompt()
        {
            _controller!.InputManager.RegisterGetPoint(
                new PromptPointOptions { Message = PromptCenterPoint },
                OnCenterPoint);
        }

        private void OnCenterPoint(PromptPointResult result)
        {
            if (result.Status != PromptStatus.OK) { Finish(); return; }
            _center = result.Value;
            RegisterRadiusPrompt();
        }

        private void RegisterRadiusPrompt()
        {
            _controller!.InputManager.RegisterGetPoint(
                new PromptPointOptions { Message = PromptRadius, BasePoint = _center },
                OnRadiusPoint);
        }

        private void OnRadiusPoint(PromptPointResult result)
        {
            if (result.Status != PromptStatus.OK) { Finish(); return; }

            _radius = _center!.Value.DistanceTo(result.Value);
            if (_radius < 1e-6) { Finish(); return; }

            _startAngle = Math.Atan2(
                result.Value.Y - _center.Value.Y,
                result.Value.X - _center.Value.X);
            if (_startAngle < 0) _startAngle += 2 * Math.PI;

            RegisterEndAnglePrompt();
        }

        private void RegisterEndAnglePrompt()
        {
            _controller!.InputManager.RegisterGetPoint(
                new PromptPointOptions { Message = PromptEndAngle, BasePoint = _center },
                OnEndAnglePoint);
        }

        private void OnEndAnglePoint(PromptPointResult result)
        {
            if (result.Status != PromptStatus.OK) { Finish(); return; }

            double endAngle = Math.Atan2(
                result.Value.Y - _center!.Value.Y,
                result.Value.X - _center.Value.X);
            if (endAngle < 0) endAngle += 2 * Math.PI;

            var arc = new Arc(_center.Value, _radius, _startAngle, endAngle)
            {
                Layer = _controller!.ActiveLayer,
                Color = _controller.ActiveColor
            };

            CadCoreHelper.AddNewEntityToCurrentSpace(arc);
            Finish();
        }

        private void OnMouseMove(Point3d worldPt)
        {
            if (_controller == null || !_center.HasValue) return;

            if (_radius == 0)
            {
                double r = _center.Value.DistanceTo(worldPt);
                _controller.InputManager.SetPreview("arcRadius",
                    new Circle(_center.Value, Vector3d.ZAxis, r)
                    {
                        Layer = _controller.ActiveLayer,
                        Color = _controller.ActiveColor
                    });
            }
            else
            {
                double endAngle = Math.Atan2(
                    worldPt.Y - _center.Value.Y, worldPt.X - _center.Value.X);
                if (endAngle < 0) endAngle += 2 * Math.PI;

                _controller.InputManager.SetPreview("arc",
                    new Arc(_center.Value, _radius, _startAngle, endAngle)
                    {
                        Layer = _controller.ActiveLayer,
                        Color = _controller.ActiveColor
                    });
            }
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
