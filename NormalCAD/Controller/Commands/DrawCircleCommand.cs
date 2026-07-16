using System;
using NormalCAD.Core.DatabaseServices;
using NormalCAD.Core.EditorInput;
using NormalCAD.Core.Geometry;
using NormalCAD.Resources;
using NormalCAD.Utilities;
using NormalCAD.View.Controls;

namespace NormalCAD.Controller.Commands
{
    public class DrawCircleCommand : ICadCommand
    {
        private static string PromptCenterPoint => CommandResources.Get("CIRCLE.PROMPT.CENTERPOINT");
        private static string PromptRadius => CommandResources.Get("CIRCLE.PROMPT.RADIUS");
        private static string PromptDiameter => CommandResources.Get("CIRCLE.PROMPT.DIAMETER");
        private static string KeyDiameter => CommandResources.Get("CIRCLE.KEY.DIAMETER");
        private static string KeyRadius => CommandResources.Get("CIRCLE.KEY.RADIUS");

        private CadController? _controller;
        private Point3d? _center;
        private bool _isDiameter;
        private Point3d _lastWorldPoint;

        public string Name => "_.CIRCLE";
        public string LocalName => CommandResources.Get("CIRCLE.LOCALNAME");
        public string Alias => CommandResources.Get("CIRCLE.ALIAS");
        public bool IsInternal => false;

        public void Activate(CadController controller)
        {
            _controller = controller;
            _controller.Viewport.CurrentCursorState = CadCursorState.Crosshair;
            _center = null;
            _isDiameter = false;
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
                new PromptPointOptions
                {
                    Message = _isDiameter ? PromptDiameter : PromptRadius,
                    BasePoint = _center,
                    Keywords = new[] { _isDiameter ? KeyRadius : KeyDiameter }
                },
                OnRadiusPoint);
        }

        private void OnRadiusPoint(PromptPointResult result)
        {
            if (result.Status == PromptStatus.Keyword)
            {
                _isDiameter = !_isDiameter;
                RegisterRadiusPrompt();
                return;
            }
            if (result.Status != PromptStatus.OK) { Finish(); return; }

            double dist = _center!.Value.DistanceTo(result.Value);
            double radius = _isDiameter ? dist / 2.0 : dist;

            if (radius > 1e-6)
            {
                var circle = new Circle(_center.Value, Vector3d.ZAxis, radius)
                {
                    Layer = _controller!.ActiveLayer,
                    Color = _controller.ActiveColor
                };
                CadCoreHelper.AddNewEntityToCurrentSpace(circle);
            }

            Finish();
        }

        private void OnMouseMove(Point3d worldPt)
        {
            _lastWorldPoint = worldPt;
            if (_controller == null || !_center.HasValue) return;

            double dist = _center.Value.DistanceTo(worldPt);
            double radius = _isDiameter ? dist / 2.0 : dist;

            if (radius > 1e-6)
            {
                _controller.InputManager.SetPreview("circle",
                    new Circle(_center.Value, Vector3d.ZAxis, radius)
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

        public void OnPointerPressed(Point3d worldPt, Avalonia.Input.PointerPressedEventArgs e) { }
        public void OnPointerMoved(Point3d worldPt) { }
        public void OnKeyDown(Avalonia.Input.KeyEventArgs e) { }
    }
}
