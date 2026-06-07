using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using NormalCAD.Core;
using NormalCAD.Core.Geometry;
using NormalCAD.Core.Entities;

namespace NormalCAD.View.Controls
{
    public class CadViewport : Control
    {
        public static readonly StyledProperty<bool> IsLightThemeProperty =
            AvaloniaProperty.Register<CadViewport, bool>(nameof(IsLightTheme), defaultValue: false);

        public bool IsLightTheme
        {
            get => GetValue(IsLightThemeProperty);
            set => SetValue(IsLightThemeProperty, value);
        }

        public Database? Database { get; set; }
        public Controller.CadController? Controller { get; set; }

        public Point3d WorldCenter { get; set; } = Point3d.Origin;
        public double Zoom { get; set; } = 1.0;

        public HashSet<ObjectId> SelectedEntityIds { get; } = new HashSet<ObjectId>();
        public Entity? ActiveCommandPreview { get; set; }

        public SnapType ActiveSnapType { get; private set; } = SnapType.None;
        public Point3d? ActiveSnapPoint { get; private set; }

        private bool _isPanning = false;
        private Point _lastMousePos;

        static CadViewport()
        {
            AffectsRender<CadViewport>(IsLightThemeProperty);
        }

        public CadViewport()
        {
            ClipToBounds = true;
            Focusable = true;
        }

        public Point WorldToScreen(Point3d worldPt)
        {
            double viewportCenterX = Bounds.Width / 2;
            double viewportCenterY = Bounds.Height / 2;

            double screenX = viewportCenterX + (worldPt.X - WorldCenter.X) * Zoom;
            double screenY = viewportCenterY - (worldPt.Y - WorldCenter.Y) * Zoom;

            return new Point(screenX, screenY);
        }

        public Point3d ScreenToWorld(Point screenPt)
        {
            double viewportCenterX = Bounds.Width / 2;
            double viewportCenterY = Bounds.Height / 2;

            double worldX = WorldCenter.X + (screenPt.X - viewportCenterX) / Zoom;
            double worldY = WorldCenter.Y - (screenPt.Y - viewportCenterY) / Zoom;

            return new Point3d(worldX, worldY, 0.0);
        }

        protected override void OnPointerPressed(PointerPressedEventArgs e)
        {
            base.OnPointerPressed(e);
            var pos = e.GetPosition(this);
            var properties = e.GetCurrentPoint(this).Properties;

            if (properties.IsMiddleButtonPressed)
            {
                _isPanning = true;
                _lastMousePos = pos;
                e.Handled = true;
            }
            else if (properties.IsLeftButtonPressed)
            {
                var snappedPt = GetSnappedPoint(pos, out var snapType, out var snapPt);
                Controller?.OnPointerPressed(snappedPt, e);
                e.Handled = true;
            }
        }

        protected override void OnPointerMoved(PointerEventArgs e)
        {
            base.OnPointerMoved(e);
            var pos = e.GetPosition(this);

            if (_isPanning)
            {
                double dx = pos.X - _lastMousePos.X;
                double dy = pos.Y - _lastMousePos.Y;
                WorldCenter = new Point3d(WorldCenter.X - dx / Zoom, WorldCenter.Y + dy / Zoom, 0);
                _lastMousePos = pos;
                InvalidateVisual();
            }
            else
            {
                var snappedPt = GetSnappedPoint(pos, out var snapType, out var snapPt);
                ActiveSnapType = snapType;
                ActiveSnapPoint = snapType != SnapType.None ? snapPt : null;

                Controller?.OnPointerMoved(snappedPt);
                InvalidateVisual();
            }
        }

        protected override void OnPointerReleased(PointerReleasedEventArgs e)
        {
            base.OnPointerReleased(e);
            if (_isPanning && e.InitialPressMouseButton == MouseButton.Middle)
            {
                _isPanning = false;
                e.Handled = true;
            }
        }

        protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
        {
            base.OnPointerWheelChanged(e);
            var screenPos = e.GetPosition(this);
            var worldPos = ScreenToWorld(screenPos);

            double scale = e.Delta.Y > 0 ? 1.15 : 1 / 1.15;
            double newZoom = Math.Clamp(Zoom * scale, 0.001, 1000.0);

            double viewportCenterX = Bounds.Width / 2;
            double viewportCenterY = Bounds.Height / 2;

            // Ajusta o WorldCenter para manter o ponto sob o mouse na mesma posição de tela
            WorldCenter = new Point3d(
                worldPos.X - (screenPos.X - viewportCenterX) / newZoom,
                worldPos.Y + (screenPos.Y - viewportCenterY) / newZoom,
                0.0
            );

            Zoom = newZoom;
            InvalidateVisual();
        }

        public override void Render(DrawingContext context)
        {
            base.Render(context);

            // 1. Desenha o Fundo
            var bgBrush = IsLightTheme ? new SolidColorBrush(Color.Parse("#F5F5F5")) : new SolidColorBrush(Color.Parse("#181818"));
            context.DrawRectangle(bgBrush, null, new Rect(0, 0, Bounds.Width, Bounds.Height));

            // 2. Desenha a Grade (Grid)
            DrawGrid(context);

            // 3. Desenha Eixos do Sistema
            DrawSystemAxes(context);

            // 4. Desenha as Entidades do Banco de Dados
            DrawDatabaseEntities(context);

            // 5. Desenha o Preview do Comando Ativo
            if (ActiveCommandPreview != null)
            {
                DrawEntity(context, ActiveCommandPreview, isSelected: false, isPreview: true);
            }

            // 6. Desenha o Indicador de Snap
            DrawSnapIndicator(context);
        }

        private void DrawGrid(DrawingContext context)
        {
            if (Bounds.Width <= 0 || Bounds.Height <= 0) return;

            var worldMin = ScreenToWorld(new Point(0, Bounds.Height));
            var worldMax = ScreenToWorld(new Point(Bounds.Width, 0));

            double minSpacingPixels = 50;
            double worldMinSpacing = minSpacingPixels / Zoom;

            double log = Math.Log10(worldMinSpacing);
            double powerOf10 = Math.Pow(10, Math.Floor(log));

            double gridSpacing;
            double ratio = worldMinSpacing / powerOf10;
            if (ratio < 2) gridSpacing = powerOf10;
            else if (ratio < 5) gridSpacing = powerOf10 * 2;
            else gridSpacing = powerOf10 * 5;

            double startX = Math.Ceiling(worldMin.X / gridSpacing) * gridSpacing;
            double startY = Math.Ceiling(worldMin.Y / gridSpacing) * gridSpacing;

            var gridColor = IsLightTheme ? Color.Parse("#E0E0E0") : Color.Parse("#262626");
            var gridPen = new Pen(new SolidColorBrush(gridColor), 0.8);

            // Linhas verticais
            for (double x = startX; x <= worldMax.X; x += gridSpacing)
            {
                if (Math.Abs(x) < 1e-6) continue;
                Point p1 = WorldToScreen(new Point3d(x, worldMin.Y));
                Point p2 = WorldToScreen(new Point3d(x, worldMax.Y));
                context.DrawLine(gridPen, p1, p2);
            }

            // Linhas horizontais
            for (double y = startY; y <= worldMax.Y; y += gridSpacing)
            {
                if (Math.Abs(y) < 1e-6) continue;
                Point p1 = WorldToScreen(new Point3d(worldMin.X, y));
                Point p2 = WorldToScreen(new Point3d(worldMax.X, y));
                context.DrawLine(gridPen, p1, p2);
            }
        }

        private void DrawSystemAxes(DrawingContext context)
        {
            var worldMin = ScreenToWorld(new Point(0, Bounds.Height));
            var worldMax = ScreenToWorld(new Point(Bounds.Width, 0));

            var xAxisPen = new Pen(Brushes.Red, 1.2);
            var yAxisPen = new Pen(Brushes.Green, 1.2);

            // Eixo Y (X = 0)
            if (worldMin.X <= 0 && worldMax.X >= 0)
            {
                Point p1 = WorldToScreen(new Point3d(0, worldMin.Y));
                Point p2 = WorldToScreen(new Point3d(0, worldMax.Y));
                context.DrawLine(yAxisPen, p1, p2);
            }

            // Eixo X (Y = 0)
            if (worldMin.Y <= 0 && worldMax.Y >= 0)
            {
                Point p1 = WorldToScreen(new Point3d(worldMin.X, 0));
                Point p2 = WorldToScreen(new Point3d(worldMax.X, 0));
                context.DrawLine(xAxisPen, p1, p2);
            }
        }

        private void DrawDatabaseEntities(DrawingContext context)
        {
            if (Database == null) return;

            if (Database.TryGetObject(Database.BlockTableId, out var btObj) && btObj is BlockTable bt)
            {
                var modelSpaceId = bt[BlockTableRecord.ModelSpace];
                if (!modelSpaceId.IsNull && Database.TryGetObject(modelSpaceId, out var btrObj) && btrObj is BlockTableRecord btr)
                {
                    foreach (var entId in btr.GetEntityIds())
                    {
                        if (Database.TryGetObject(entId, out var entObj) && entObj is Entity ent)
                        {
                            DrawEntity(context, ent, SelectedEntityIds.Contains(entId));
                        }
                    }
                }
            }
        }

        private void DrawEntity(DrawingContext context, Entity ent, bool isSelected, bool isPreview = false)
        {
            var baseColor = GetEntityRenderColor(ent);
            Color renderColor = isSelected ? Color.Parse("#007ACC") : baseColor;
            if (isPreview)
            {
                renderColor = Color.Parse("#FF9900");
            }

            var brush = new SolidColorBrush(renderColor);
            var pen = new Pen(brush, isSelected ? 3.0 : (isPreview ? 1.0 : 1.5));
            if (isPreview)
            {
                pen.DashStyle = DashStyle.Dash;
            }

            if (ent is Line line)
            {
                context.DrawLine(pen, WorldToScreen(line.StartPoint), WorldToScreen(line.EndPoint));
            }
            else if (ent is Circle circle)
            {
                context.DrawEllipse(null, pen, WorldToScreen(circle.Center), circle.Radius * Zoom, circle.Radius * Zoom);
            }
            else if (ent is Arc arc)
            {
                var arcGeom = CreateArcGeometry(WorldToScreen(arc.Center), arc.Radius * Zoom, arc.StartAngle, arc.EndAngle);
                context.DrawGeometry(null, pen, arcGeom);
            }
        }

        private Color GetEntityRenderColor(Entity ent)
        {
            EntityColor coreColor = ent.Color;
            if (coreColor.IsByLayer && Database != null)
            {
                if (Database.TryGetObject(Database.LayerTableId, out var ltObj) && ltObj is LayerTable lt)
                {
                    var layerId = lt[ent.Layer];
                    if (!layerId.IsNull)
                    {
                        var layer = lt.GetRecord(layerId);
                        coreColor = layer.Color;
                    }
                }
            }

            if (coreColor.IsByLayer)
                return IsLightTheme ? Colors.Black : Colors.White;

            return Color.FromArgb(coreColor.A, coreColor.R, coreColor.G, coreColor.B);
        }

        private Geometry CreateArcGeometry(Point center, double radius, double startAngleDeg, double endAngleDeg)
        {
            if (endAngleDeg < startAngleDeg)
                endAngleDeg += 360.0;

            double sweepAngle = endAngleDeg - startAngleDeg;
            if (sweepAngle >= 360.0)
            {
                return new EllipseGeometry(new Rect(center.X - radius, center.Y - radius, radius * 2, radius * 2));
            }

            double startRad = startAngleDeg * Math.PI / 180.0;
            double endRad = endAngleDeg * Math.PI / 180.0;

            Point startPt = new Point(
                center.X + radius * Math.Cos(startRad),
                center.Y - radius * Math.Sin(startRad) // Y invertido para tela
            );

            Point endPt = new Point(
                center.X + radius * Math.Cos(endRad),
                center.Y - radius * Math.Sin(endRad) // Y invertido para tela
            );

            var pathGeometry = new PathGeometry();
            var pathFigure = new PathFigure
            {
                StartPoint = startPt,
                IsClosed = false
            };

            bool isLargeArc = sweepAngle > 180.0;

            var arcSegment = new ArcSegment
            {
                Point = endPt,
                Size = new Size(radius, radius),
                RotationAngle = 0,
                IsLargeArc = isLargeArc,
                SweepDirection = SweepDirection.CounterClockwise // No CAD, o padrão do arco é anti-horário
            };

            pathFigure.Segments.Add(arcSegment);
            pathGeometry.Figures.Add(pathFigure);

            return pathGeometry;
        }

        private void DrawSnapIndicator(DrawingContext context)
        {
            if (ActiveSnapType == SnapType.None || !ActiveSnapPoint.HasValue) return;

            Point screenSnap = WorldToScreen(ActiveSnapPoint.Value);
            var snapPen = new Pen(Brushes.Lime, 2.0);
            double size = 12;
            double half = size / 2;

            if (ActiveSnapType == SnapType.Endpoint)
            {
                context.DrawRectangle(null, snapPen, new Rect(screenSnap.X - half, screenSnap.Y - half, size, size));
            }
            else if (ActiveSnapType == SnapType.Midpoint)
            {
                var geometry = new PathGeometry();
                var figure = new PathFigure { StartPoint = new Point(screenSnap.X, screenSnap.Y - half), IsClosed = true };
                figure.Segments.Add(new LineSegment { Point = new Point(screenSnap.X - half, screenSnap.Y + half) });
                figure.Segments.Add(new LineSegment { Point = new Point(screenSnap.X + half, screenSnap.Y + half) });
                geometry.Figures.Add(figure);
                context.DrawGeometry(null, snapPen, geometry);
            }
            else if (ActiveSnapType == SnapType.Center)
            {
                context.DrawEllipse(null, snapPen, screenSnap, half, half);
            }
        }

        private Point3d GetSnappedPoint(Point screenMousePos, out SnapType activeSnap, out Point3d snapPoint)
        {
            activeSnap = SnapType.None;
            snapPoint = Point3d.Origin;

            if (Database == null)
                return ScreenToWorld(screenMousePos);

            double snapAperturePixels = 15;
            double bestDistance = snapAperturePixels;

            if (!Database.TryGetObject(Database.BlockTableId, out var btObj) || btObj is not BlockTable bt)
                return ScreenToWorld(screenMousePos);

            ObjectId modelSpaceId = bt[BlockTableRecord.ModelSpace];
            if (modelSpaceId.IsNull || !Database.TryGetObject(modelSpaceId, out var btrObj) || btrObj is not BlockTableRecord modelSpace)
                return ScreenToWorld(screenMousePos);

            foreach (var entId in modelSpace.GetEntityIds())
            {
                if (!Database.TryGetObject(entId, out var entObj) || entObj is not Entity ent)
                    continue;

                if (ent is Line line)
                {
                    CheckSnapCandidate(screenMousePos, line.StartPoint, SnapType.Endpoint, ref activeSnap, ref snapPoint, ref bestDistance);
                    CheckSnapCandidate(screenMousePos, line.EndPoint, SnapType.Endpoint, ref activeSnap, ref snapPoint, ref bestDistance);

                    Point3d midPt = new Point3d(
                        (line.StartPoint.X + line.EndPoint.X) / 2,
                        (line.StartPoint.Y + line.EndPoint.Y) / 2,
                        (line.StartPoint.Z + line.EndPoint.Z) / 2
                    );
                    CheckSnapCandidate(screenMousePos, midPt, SnapType.Midpoint, ref activeSnap, ref snapPoint, ref bestDistance);
                }
                else if (ent is Circle circle)
                {
                    CheckSnapCandidate(screenMousePos, circle.Center, SnapType.Center, ref activeSnap, ref snapPoint, ref bestDistance);
                }
                else if (ent is Arc arc)
                {
                    CheckSnapCandidate(screenMousePos, arc.Center, SnapType.Center, ref activeSnap, ref snapPoint, ref bestDistance);

                    double startRad = arc.StartAngle * Math.PI / 180.0;
                    double endRad = arc.EndAngle * Math.PI / 180.0;
                    Point3d startPt = new Point3d(
                        arc.Center.X + arc.Radius * Math.Cos(startRad),
                        arc.Center.Y + arc.Radius * Math.Sin(startRad),
                        arc.Center.Z
                    );
                    Point3d endPt = new Point3d(
                        arc.Center.X + arc.Radius * Math.Cos(endRad),
                        arc.Center.Y + arc.Radius * Math.Sin(endRad),
                        arc.Center.Z
                    );

                    CheckSnapCandidate(screenMousePos, startPt, SnapType.Endpoint, ref activeSnap, ref snapPoint, ref bestDistance);
                    CheckSnapCandidate(screenMousePos, endPt, SnapType.Endpoint, ref activeSnap, ref snapPoint, ref bestDistance);
                }
            }

            if (activeSnap != SnapType.None)
            {
                return snapPoint;
            }

            return ScreenToWorld(screenMousePos);
        }

        private void CheckSnapCandidate(Point screenMousePos, Point3d worldCandidate, SnapType type, ref SnapType activeSnap, ref Point3d snapPoint, ref double bestDistance)
        {
            Point screenCandidate = WorldToScreen(worldCandidate);
            double dx = screenMousePos.X - screenCandidate.X;
            double dy = screenMousePos.Y - screenCandidate.Y;
            double dist = Math.Sqrt(dx * dx + dy * dy);

            if (dist < bestDistance)
            {
                bestDistance = dist;
                activeSnap = type;
                snapPoint = worldCandidate;
            }
        }
    }
}
