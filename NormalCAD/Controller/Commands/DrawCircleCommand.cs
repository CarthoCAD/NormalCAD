using Avalonia.Input;
using NormalCAD.Core.Geometry;
using NormalCAD.Core.DatabaseServices;
using System;
using NormalCAD.View.Controls;

namespace NormalCAD.Controller.Commands
{
    public class DrawCircleCommand : ICadCommand
    {
        private CadController? _controller;
        private Point3d? _center;
        private bool _isDiameter;
        private Point3d _lastWorldPoint;

        public string Name => "_.CIRCLE";
        public string LocalName => "CIRCLE";
        public string Alias => "C,CI";
        public bool IsInternal => false;

        public void Activate(CadController controller)
        {
            _controller = controller;
            _controller.Viewport.CurrentCursorState = CadCursorState.Crosshair;
            _center = null;
            _isDiameter = false;

            _controller.InputManager.SetCurrentPrompt(LocalName, "Specify center point for circle ");
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

            if (keyword == "Diameter")
            {
                _isDiameter = true;
                _controller.InputManager.SetCurrentPrompt(LocalName, "Specify diameter of circle ",
                    new[] { "Radius" }, OnKeyword);
            }
            else if (keyword == "Radius")
            {
                _isDiameter = false;
                _controller.InputManager.SetCurrentPrompt(LocalName, "Specify radius of circle ",
                    new[] { "Diameter" }, OnKeyword);
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

                _controller.InputManager.SetCurrentPrompt(LocalName, "Specify radius of circle ",
                    new[] { "Diameter" }, OnKeyword);
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
