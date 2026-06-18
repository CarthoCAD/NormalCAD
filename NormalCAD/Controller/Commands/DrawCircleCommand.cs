using Avalonia.Input;
using NormalCAD.Core;
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
                    using (var trans = _controller.Database.TransactionManager.StartTransaction())
                    {
                        if (_controller.Database.TryGetObject(_controller.Database.BlockTableId, out var btObj) && btObj is BlockTable bt)
                        {
                            var modelSpaceId = bt[BlockTableRecord.ModelSpace];
                            if (_controller.Database.TryGetObject(modelSpaceId, out var btrObj) && btrObj is BlockTableRecord btr)
                            {
                                var circle = new Circle(_center.Value, radius)
                                {
                                    Layer = _controller.ActiveLayer,
                                    Color = _controller.ActiveColor
                                };
                                btr.AppendEntity(circle);
                                trans.AddNewlyCreatedDBObject(circle, true);
                            }
                        }
                        trans.Commit();
                    }

                    _controller.NotifyDatabaseChanged();
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
                Color = _controller.GetResolvedColor()
            };
        }

        public void OnKeyDown(KeyEventArgs e)
        {
        }
    }
}
