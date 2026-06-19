using Avalonia.Input;
using NormalCAD.Core.Geometry;
using NormalCAD.Core.Entities;
using System;
using NormalCAD.View.Controls;

namespace NormalCAD.Controller.Commands
{
    public class DrawCircleCommand : ICadCommand
    {
        private CadController? _controller;
        private Point3d? _center;

        public string Name => "_.CIRCLE";
        public string LocalName => "CIRCLE";
        public string Alias => "C,CI";
        public bool IsInternal => false;

        public void Activate(CadController controller)
        {
            _controller = controller;
            _controller.Viewport.CurrentCursorState = CadCursorState.Crosshair;
            _center = null;
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

            if (!_center.HasValue)
            {
                _center = worldPt;
            }
            else
            {
                double radius = _center.Value.DistanceTo(worldPt);
                if (radius > 1e-6)
                {
                    var circle = new Circle(_center.Value, radius)
                    {
                        Layer = _controller.ActiveLayer,
                        Color = _controller.ActiveColor
                    };
                    _controller.AddNewEntityToActiveSpace(circle);
                }

                _center = null;
                _controller.Viewport.ActiveCommandPreview = null;
            }
        }

        public void OnPointerMoved(Point3d worldPt)
        {
            if (_controller == null || !_center.HasValue) return;

            double radius = _center.Value.DistanceTo(worldPt);
            _controller.Viewport.ActiveCommandPreview = new Circle(_center.Value, radius)
            {
                Layer = _controller.ActiveLayer,
                Color = _controller.ActiveColor
            };
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
