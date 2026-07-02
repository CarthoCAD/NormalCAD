using Avalonia.Input;
using NormalCAD.Core.Geometry;
using NormalCAD.Core.DatabaseServices;
using NormalCAD.Resources;
using System;
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

            _controller.InputManager.SetCurrentPrompt(LocalName, PromptCenterPoint);
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

        private void OnKeyword(string keyword)
        {
            if (_controller == null || !_center.HasValue) return;

            if (keyword == KeyDiameter)
            {
                _isDiameter = true;
                _controller.InputManager.SetCurrentPrompt(LocalName, PromptDiameter,
                    new[] { KeyRadius }, OnKeyword);
            }
            else if (keyword == KeyRadius)
            {
                _isDiameter = false;
                _controller.InputManager.SetCurrentPrompt(LocalName, PromptRadius,
                    new[] { KeyDiameter }, OnKeyword);
            }

            UpdatePreview();
            _controller.Viewport.InvalidateVisual();
        }

        public void OnPointerPressed(Point3d worldPt, PointerPressedEventArgs e)
        {
            if (_controller == null) return;

            if (!_center.HasValue)
            {
                _center = worldPt;

                _controller.InputManager.SetCurrentPrompt(LocalName, PromptRadius,
                    new[] { KeyDiameter }, OnKeyword);
            }
            else
            {
                double dist = _center.Value.DistanceTo(worldPt);
                double radius = _isDiameter ? dist / 2.0 : dist;

                if (radius > 1e-6)
                {
                    var circle = new Circle(_center.Value, radius)
                    {
                        Layer = _controller.ActiveLayer,
                        Color = _controller.ActiveColor
                    };
                    _controller.AddNewEntityToActiveSpace(circle);
                }

                _controller.SetCommand(new BaseCommand());
            }
        }

        public void OnPointerMoved(Point3d worldPt)
        {
            _lastWorldPoint = worldPt;
            UpdatePreview();
        }

        private void UpdatePreview()
        {
            if (_controller == null || !_center.HasValue) return;

            double dist = _center.Value.DistanceTo(_lastWorldPoint);
            double radius = _isDiameter ? dist / 2.0 : dist;

            if (radius > 1e-6)
            {
                _controller.Viewport.ActiveCommandPreview = new Circle(_center.Value, radius)
                {
                    Layer = _controller.ActiveLayer,
                    Color = _controller.ActiveColor
                };
            }
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
