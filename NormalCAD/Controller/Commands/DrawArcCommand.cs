using System;
using Avalonia.Input;
using NormalCAD.Core.Geometry;
using NormalCAD.Core.Entities;
using NormalCAD.View.Controls;

namespace NormalCAD.Controller.Commands
{
    public class DrawArcCommand : ICadCommand
    {
        private CadController? _controller;
        private Point3d? _center;
        private double _radius;
        private double _startAngle;

        public string Name => "_.ARC";
        public string LocalName => "ARC";
        public string Alias => "A";
        public bool IsInternal => false;

        public void Activate(CadController controller)
        {
            _controller = controller;
            _controller.Viewport.CurrentCursorState = CadCursorState.Crosshair;
            _center = null;
            _radius = 0;
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
            else if (_radius == 0)
            {
                _radius = _center.Value.DistanceTo(worldPt);
                if (_radius < 1e-6) return;
                _startAngle = Math.Atan2(worldPt.Y - _center.Value.Y, worldPt.X - _center.Value.X) * 180.0 / Math.PI;
                if (_startAngle < 0) _startAngle += 360;
            }
            else
            {
                double endAngle = Math.Atan2(worldPt.Y - _center.Value.Y, worldPt.X - _center.Value.X) * 180.0 / Math.PI;
                if (endAngle < 0) endAngle += 360;

                var arc = new Arc(_center.Value, _radius, _startAngle, endAngle)
                {
                    Layer = _controller.ActiveLayer,
                    Color = _controller.ActiveColor
                };
                _controller.AddNewEntityToActiveSpace(arc);
                _controller.SetCommand(new BaseCommand());
            }
        }

        public void OnPointerMoved(Point3d worldPt)
        {
            if (_controller == null || !_center.HasValue) return;

            if (_radius == 0)
            {
                double r = _center.Value.DistanceTo(worldPt);
                var previewCenter = _center.Value;
                _controller.Viewport.ActiveCommandPreview = new Circle(previewCenter, r)
                {
                    Layer = _controller.ActiveLayer,
                    Color = _controller.ActiveColor
                };
            }
            else
            {
                double endAngle = Math.Atan2(worldPt.Y - _center.Value.Y, worldPt.X - _center.Value.X) * 180.0 / Math.PI;
                if (endAngle < 0) endAngle += 360;
                _controller.Viewport.ActiveCommandPreview = new Arc(_center.Value, _radius, _startAngle, endAngle)
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
