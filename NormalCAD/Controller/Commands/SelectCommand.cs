using Avalonia.Input;
using System;
using NormalCAD.Core;
using NormalCAD.Core.Geometry;
using NormalCAD.Core.Entities;

namespace NormalCAD.Controller.Commands
{
    public class SelectCommand : ICadCommand
    {
        private CadController? _controller;

        public string Name => "Seleção";

        public void Activate(CadController controller)
        {
            _controller = controller;
        }

        public void Deactivate()
        {
        }

        public void OnPointerPressed(Point3d worldPt, PointerPressedEventArgs e)
        {
            if (_controller == null) return;

            var viewport = _controller.Viewport;
            var db = _controller.Database;
            var mouseScreenPos = e.GetPosition(viewport);

            ObjectId selectedId = ObjectId.Null;
            double bestDist = 10.0; // tolerância de 10 pixels

            if (db.TryGetObject(db.BlockTableId, out var btObj) && btObj is BlockTable bt)
            {
                var modelSpaceId = bt[BlockTableRecord.ModelSpace];
                if (!modelSpaceId.IsNull && db.TryGetObject(modelSpaceId, out var btrObj) && btrObj is BlockTableRecord btr)
                {
                    foreach (var entId in btr.GetEntityIds())
                    {
                        if (!db.TryGetObject(entId, out var entObj) || entObj is not Entity ent)
                            continue;

                        double dist = GetDistanceToEntityScreen(viewport, mouseScreenPos, ent);
                        if (dist < bestDist)
                        {
                            bestDist = dist;
                            selectedId = entId;
                        }
                    }
                }
            }

            if (!selectedId.IsNull)
            {
                bool isCtrl = (e.KeyModifiers & KeyModifiers.Control) != 0;
                if (!isCtrl)
                {
                    viewport.SelectedEntityIds.Clear();
                }

                if (viewport.SelectedEntityIds.Contains(selectedId))
                {
                    viewport.SelectedEntityIds.Remove(selectedId);
                }
                else
                {
                    viewport.SelectedEntityIds.Add(selectedId);
                }
            }
            else
            {
                if ((e.KeyModifiers & KeyModifiers.Control) == 0)
                {
                    viewport.SelectedEntityIds.Clear();
                }
            }

            _controller.NotifySelectionChanged();
            viewport.InvalidateVisual();
        }

        public void OnPointerMoved(Point3d worldPt)
        {
        }

        public void OnKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Delete && _controller != null)
            {
                var selected = _controller.Viewport.SelectedEntityIds;
                if (selected.Count > 0)
                {
                    using (var trans = _controller.Database.TransactionManager.StartTransaction())
                    {
                        if (_controller.Database.TryGetObject(_controller.Database.BlockTableId, out var btObj) && btObj is BlockTable bt)
                        {
                            var modelSpaceId = bt[BlockTableRecord.ModelSpace];
                            if (_controller.Database.TryGetObject(modelSpaceId, out var btrObj) && btrObj is BlockTableRecord btr)
                            {
                                foreach (var entId in selected)
                                {
                                    btr.RemoveEntity(entId);
                                }
                            }
                        }
                        trans.Commit();
                    }
                    selected.Clear();
                    _controller.NotifySelectionChanged();
                    _controller.NotifyDatabaseChanged();
                }
            }
        }

        private double GetDistanceToEntityScreen(View.Controls.CadViewport viewport, Avalonia.Point p, Entity ent)
        {
            if (ent is Line line)
            {
                var screenStart = viewport.WorldToScreen(line.StartPoint);
                var screenEnd = viewport.WorldToScreen(line.EndPoint);
                return DistanceToSegment(p, screenStart, screenEnd);
            }
            else if (ent is Circle circle)
            {
                var screenCenter = viewport.WorldToScreen(circle.Center);
                double screenRadius = circle.Radius * viewport.Zoom;
                double dist = Math.Sqrt((p.X - screenCenter.X) * (p.X - screenCenter.X) + (p.Y - screenCenter.Y) * (p.Y - screenCenter.Y));
                return Math.Abs(dist - screenRadius);
            }
            else if (ent is Arc arc)
            {
                var screenCenter = viewport.WorldToScreen(arc.Center);
                double screenRadius = arc.Radius * viewport.Zoom;
                double dist = Math.Sqrt((p.X - screenCenter.X) * (p.X - screenCenter.X) + (p.Y - screenCenter.Y) * (p.Y - screenCenter.Y));
                
                double hitDist = Math.Abs(dist - screenRadius);
                if (hitDist > 10.0) return double.MaxValue;

                double dx = p.X - screenCenter.X;
                double dy = p.Y - screenCenter.Y;
                double clickAngle = Math.Atan2(-dy, dx) * 180.0 / Math.PI;
                if (clickAngle < 0) clickAngle += 360.0;

                double start = arc.StartAngle;
                double end = arc.EndAngle;
                if (end < start) end += 360.0;

                if (clickAngle < start) clickAngle += 360.0;

                if (clickAngle >= start && clickAngle <= end)
                    return hitDist;

                return double.MaxValue;
            }

            return double.MaxValue;
        }

        private double DistanceToSegment(Avalonia.Point p, Avalonia.Point a, Avalonia.Point b)
        {
            double l2 = (a.X - b.X) * (a.X - b.X) + (a.Y - b.Y) * (a.Y - b.Y);
            if (l2 == 0) return Math.Sqrt((p.X - a.X) * (p.X - a.X) + (p.Y - a.Y) * (p.Y - a.Y));
            double t = Math.Max(0, Math.Min(1, ((p.X - a.X) * (b.X - a.X) + (p.Y - a.Y) * (b.Y - a.Y)) / l2));
            double projX = a.X + t * (b.X - a.X);
            double projY = a.Y + t * (b.Y - a.Y);
            return Math.Sqrt((p.X - projX) * (p.X - projX) + (p.Y - projY) * (p.Y - projY));
        }
    }
}
