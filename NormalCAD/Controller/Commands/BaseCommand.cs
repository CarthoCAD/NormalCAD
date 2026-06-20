using Avalonia;
using Avalonia.Input;
using System;
using System.Collections.Generic;
using NormalCAD.Core.DatabaseServices;
using NormalCAD.Core.Geometry;
using NormalCAD.View.Controls;

namespace NormalCAD.Controller.Commands
{
    public class BaseCommand : ICadCommand
    {
        private CadController? _controller;
        private bool _isSelectingBox = false;

        public string Name => "*BASECOMMAND";
        public string LocalName => "CMD";
        public string Alias => "";
        public bool IsInternal => true;

        public void Activate(CadController controller)
        {
            _controller = controller;
            _isSelectingBox = false;
            if (_controller?.Viewport != null)
            {
                _controller.Viewport.CurrentCursorState = CadCursorState.PickCross;
                _controller.Viewport.SelectionStartPoint = null;
                _controller.Viewport.SelectionEndPoint = null;
            }
        }

        public void Deactivate()
        {
            if (_controller?.Viewport != null)
            {
                _controller.Viewport.SelectionStartPoint = null;
                _controller.Viewport.SelectionEndPoint = null;
            }
        }

        public void OnPointerPressed(Point3d worldPt, PointerPressedEventArgs e)
        {
            if (_controller == null) return;

            var viewport = _controller.Viewport;
            var db = _controller.Database;
            var mouseScreenPos = e.GetPosition(viewport);
            bool isCtrl = (e.KeyModifiers & KeyModifiers.Control) != 0;

            if (_isSelectingBox)
            {
                // Segundo clique: Finaliza o retângulo de seleção
                viewport.SelectionEndPoint = mouseScreenPos;
                PerformSelectionBox(viewport, isCtrl);

                _isSelectingBox = false;
                viewport.SelectionStartPoint = null;
                viewport.SelectionEndPoint = null;
                viewport.InvalidateVisual();
                return;
            }

            // Primeiro clique: tenta selecionar uma entidade individual sob o cursor
            ObjectId selectedId = ObjectId.Null;
            double bestDist = 10.0; // tolerância de 10 pixels

            if (db.TryGetObject(db.BlockTableId, out var btObj) && btObj is BlockTable bt)
            {
                var modelSpaceId = bt[BlockTableRecord.ModelSpace];
                if (!modelSpaceId.IsNull && db.TryGetObject(modelSpaceId, out var btrObj) && btrObj is BlockTableRecord btr)
                {
                    double zoom = Math.Max(viewport.Zoom, 0.001);
                    double worldTolerance = 100.0 / zoom;
                    Point3d worldMouse = viewport.ScreenToWorld(mouseScreenPos);
                    var candidateIds = btr.QueryNearPoint(worldMouse, worldTolerance);

                    foreach (var entId in candidateIds)
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
                if (!isCtrl)
                {
                    _controller.ClearSelection();
                }

                if (_controller.IsSelected(selectedId))
                {
                    _controller.RemoveFromSelection(selectedId);
                }
                else
                {
                    _controller.AddToSelection(selectedId);
                }

                viewport.InvalidateVisual();
            }
            else
            {
                // Clicou no vazio: Inicia a seleção por retângulo (Crossing / Window)
                _isSelectingBox = true;
                viewport.SelectionStartPoint = mouseScreenPos;
                viewport.SelectionEndPoint = mouseScreenPos;
                viewport.InvalidateVisual();
            }
        }

        public void OnPointerMoved(Point3d worldPt)
        {
            if (_controller == null || !_isSelectingBox) return;

            var viewport = _controller.Viewport;
            // O worldPt representa as coordenadas do mouse no mundo, então convertemos de volta
            // para obter a posição atual do mouse na tela de forma precisa
            viewport.SelectionEndPoint = viewport.WorldToScreen(worldPt);
        }

        public void OnKeyDown(KeyEventArgs e)
        {
        }

        private void PerformSelectionBox(CadViewport viewport, bool isCtrl)
        {
            if (!viewport.SelectionStartPoint.HasValue || !viewport.SelectionEndPoint.HasValue) return;

            var p1 = viewport.SelectionStartPoint.Value;
            var p2 = viewport.SelectionEndPoint.Value;

            var rect = GetRect(p1, p2);
            bool isCrossing = p2.X < p1.X;

            var db = _controller!.Database;
            var toSelect = new List<ObjectId>();

            if (db.TryGetObject(db.BlockTableId, out var btObj) && btObj is BlockTable bt)
            {
                var modelSpaceId = bt[BlockTableRecord.ModelSpace];
                if (!modelSpaceId.IsNull && db.TryGetObject(modelSpaceId, out var btrObj) && btrObj is BlockTableRecord btr)
                {
                    var worldTL = viewport.ScreenToWorld(rect.TopLeft);
                    var worldBR = viewport.ScreenToWorld(rect.BottomRight);
                    var worldBounds = Extents3d.FromPoints(worldTL, worldBR);
                    var candidateIds = btr.QueryExtents(worldBounds);

                    foreach (var entId in candidateIds)
                    {
                        if (!db.TryGetObject(entId, out var entObj) || entObj is not Entity ent)
                            continue;

                        if (isCrossing)
                        {
                            if (EntityIntersectsRect(viewport, rect, ent))
                            {
                                toSelect.Add(entId);
                            }
                        }
                        else
                        {
                            if (EntityInsideRect(viewport, rect, ent))
                            {
                                toSelect.Add(entId);
                            }
                        }
                    }
                }
            }

            if (!isCtrl)
            {
                _controller.ClearSelection();
            }

            foreach (var entId in toSelect)
            {
                if (isCtrl && _controller.IsSelected(entId))
                {
                    _controller.RemoveFromSelection(entId);
                }
                else
                {
                    _controller.AddToSelection(entId);
                }
            }
        }

        private Rect GetRect(Point p1, Point p2)
        {
            double x = Math.Min(p1.X, p2.X);
            double y = Math.Min(p1.Y, p2.Y);
            double w = Math.Abs(p1.X - p2.X);
            double h = Math.Abs(p1.Y - p2.Y);
            return new Rect(x, y, w, h);
        }

        private bool SegmentsIntersect(Point p1, Point p2, Point p3, Point p4)
        {
            double d = (p4.Y - p3.Y) * (p2.X - p1.X) - (p4.X - p3.X) * (p2.Y - p1.Y);
            if (Math.Abs(d) < 1e-9) return false;

            double t = ((p4.X - p3.X) * (p1.Y - p3.Y) - (p4.Y - p3.Y) * (p1.X - p3.X)) / d;
            double u = ((p2.X - p1.X) * (p1.Y - p3.Y) - (p2.Y - p1.Y) * (p1.X - p3.X)) / d;

            return t >= 0 && t <= 1 && u >= 0 && u <= 1;
        }

        private double DistanceToSegment(Point p, Point a, Point b)
        {
            double l2 = (a.X - b.X) * (a.X - b.X) + (a.Y - b.Y) * (a.Y - b.Y);
            if (l2 == 0) return Math.Sqrt((p.X - a.X) * (p.X - a.X) + (p.Y - a.Y) * (p.Y - a.Y));
            double t = Math.Max(0, Math.Min(1, ((p.X - a.X) * (b.X - a.X) + (p.Y - a.Y) * (b.Y - a.Y)) / l2));
            double projX = a.X + t * (b.X - a.X);
            double projY = a.Y + t * (b.Y - a.Y);
            return Math.Sqrt((p.X - projX) * (p.X - projX) + (p.Y - projY) * (p.Y - projY));
        }

        private bool EntityInsideRect(CadViewport viewport, Rect rect, Entity ent)
        {
            if (ent is Line line)
            {
                var pStart = viewport.WorldToScreen(line.StartPoint);
                var pEnd = viewport.WorldToScreen(line.EndPoint);
                return rect.Contains(pStart) && rect.Contains(pEnd);
            }
            else if (ent is Circle circle)
            {
                var pCenter = viewport.WorldToScreen(circle.Center);
                double r = circle.Radius * viewport.Zoom;
                return pCenter.X - r >= rect.Left && pCenter.X + r <= rect.Right &&
                       pCenter.Y - r >= rect.Top && pCenter.Y + r <= rect.Bottom;
            }
            else if (ent is Arc arc)
            {
                int numSamples = 16;
                double start = arc.StartAngle;
                double end = arc.EndAngle;
                if (end < start) end += 360.0;
                double sweep = end - start;

                for (int i = 0; i <= numSamples; i++)
                {
                    double angleDeg = start + sweep * ((double)i / numSamples);
                    double angleRad = angleDeg * Math.PI / 180.0;
                    var worldPt = new Point3d(
                        arc.Center.X + arc.Radius * Math.Cos(angleRad),
                        arc.Center.Y + arc.Radius * Math.Sin(angleRad),
                        arc.Center.Z
                    );
                    var pScreen = viewport.WorldToScreen(worldPt);
                    if (!rect.Contains(pScreen)) return false;
                }
                return true;
            }
            else if (ent is Polyline poly)
            {
                for (int i = 0; i < poly.NumberOfVertices; i++)
                {
                    if (!rect.Contains(viewport.WorldToScreen(poly.GetPoint3dAt(i)))) return false;
                }
                return true;
            }
            return false;
        }

        private bool EntityIntersectsRect(CadViewport viewport, Rect rect, Entity ent)
        {
            if (EntityInsideRect(viewport, rect, ent)) return true;

            var topLeft = rect.TopLeft;
            var topRight = rect.TopRight;
            var bottomLeft = rect.BottomLeft;
            var bottomRight = rect.BottomRight;

            if (ent is Line line)
            {
                var pStart = viewport.WorldToScreen(line.StartPoint);
                var pEnd = viewport.WorldToScreen(line.EndPoint);

                if (SegmentsIntersect(pStart, pEnd, topLeft, topRight) ||
                    SegmentsIntersect(pStart, pEnd, topRight, bottomRight) ||
                    SegmentsIntersect(pStart, pEnd, bottomRight, bottomLeft) ||
                    SegmentsIntersect(pStart, pEnd, bottomLeft, topLeft))
                {
                    return true;
                }

                if (rect.Contains(pStart) || rect.Contains(pEnd))
                {
                    return true;
                }
            }
            else if (ent is Circle circle)
            {
                var pCenter = viewport.WorldToScreen(circle.Center);
                double r = circle.Radius * viewport.Zoom;

                if (rect.Contains(pCenter)) return true;

                if (DistanceToSegment(pCenter, topLeft, topRight) <= r ||
                    DistanceToSegment(pCenter, topRight, bottomRight) <= r ||
                    DistanceToSegment(pCenter, bottomRight, bottomLeft) <= r ||
                    DistanceToSegment(pCenter, bottomLeft, topLeft) <= r)
                {
                    return true;
                }
            }
            else if (ent is Arc arc)
            {
                int numSamples = 16;
                double start = arc.StartAngle;
                double end = arc.EndAngle;
                if (end < start) end += 360.0;
                double sweep = end - start;

                Point? lastScreenPt = null;

                for (int i = 0; i <= numSamples; i++)
                {
                    double angleDeg = start + sweep * ((double)i / numSamples);
                    double angleRad = angleDeg * Math.PI / 180.0;
                    var worldPt = new Point3d(
                        arc.Center.X + arc.Radius * Math.Cos(angleRad),
                        arc.Center.Y + arc.Radius * Math.Sin(angleRad),
                        arc.Center.Z
                    );
                    var pScreen = viewport.WorldToScreen(worldPt);

                    if (rect.Contains(pScreen)) return true;

                    if (lastScreenPt.HasValue)
                    {
                        if (SegmentsIntersect(lastScreenPt.Value, pScreen, topLeft, topRight) ||
                            SegmentsIntersect(lastScreenPt.Value, pScreen, topRight, bottomRight) ||
                            SegmentsIntersect(lastScreenPt.Value, pScreen, bottomRight, bottomLeft) ||
                            SegmentsIntersect(lastScreenPt.Value, pScreen, bottomLeft, topLeft))
                        {
                            return true;
                        }
                    }

                    lastScreenPt = pScreen;
                }
            }
            else if (ent is Polyline poly)
            {
                for (int i = 0; i < poly.NumberOfVertices - 1; i++)
                {
                    var p1 = viewport.WorldToScreen(poly.GetPoint3dAt(i));
                    var p2 = viewport.WorldToScreen(poly.GetPoint3dAt(i + 1));

                    if (rect.Contains(p1) || rect.Contains(p2)) return true;

                    if (SegmentsIntersect(p1, p2, topLeft, topRight) ||
                        SegmentsIntersect(p1, p2, topRight, bottomRight) ||
                        SegmentsIntersect(p1, p2, bottomRight, bottomLeft) ||
                        SegmentsIntersect(p1, p2, bottomLeft, topLeft))
                        return true;
                }

                if (poly.Closed && poly.NumberOfVertices > 1)
                {
                    var p1 = viewport.WorldToScreen(poly.GetPoint3dAt(poly.NumberOfVertices - 1));
                    var p2 = viewport.WorldToScreen(poly.GetPoint3dAt(0));
                    if (rect.Contains(p1) || rect.Contains(p2)) return true;
                    if (SegmentsIntersect(p1, p2, topLeft, topRight) ||
                        SegmentsIntersect(p1, p2, topRight, bottomRight) ||
                        SegmentsIntersect(p1, p2, bottomRight, bottomLeft) ||
                        SegmentsIntersect(p1, p2, bottomLeft, topLeft))
                        return true;
                }
            }
            return false;
        }

        private double GetDistanceToEntityScreen(CadViewport viewport, Point p, Entity ent)
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
            else if (ent is Polyline poly)
            {
                double best = double.MaxValue;
                for (int i = 0; i < poly.NumberOfVertices - 1; i++)
                {
                    var p1 = viewport.WorldToScreen(poly.GetPoint3dAt(i));
                    var p2 = viewport.WorldToScreen(poly.GetPoint3dAt(i + 1));
                    double d = DistanceToSegment(p, p1, p2);
                    if (d < best) best = d;
                }
                if (poly.Closed && poly.NumberOfVertices > 1)
                {
                    var p1 = viewport.WorldToScreen(poly.GetPoint3dAt(poly.NumberOfVertices - 1));
                    var p2 = viewport.WorldToScreen(poly.GetPoint3dAt(0));
                    double d = DistanceToSegment(p, p1, p2);
                    if (d < best) best = d;
                }
                return best;
            }

            return double.MaxValue;
        }
    }
}
