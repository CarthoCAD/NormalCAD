using System;
using Avalonia;
using Avalonia.Media;
using NormalCAD.Core.DatabaseServices;
using NormalCAD.Core.Geometry;

namespace NormalCAD.View.Drawing
{
    public class ArcRenderer : IEntityRenderer
    {
        public void Render(DrawingContext context, Entity entity, Pen pen,
                           Func<Point3d, Point> worldToScreen, double zoom)
        {
            if (entity is not Arc arc) return;
            var center = worldToScreen(arc.Center);
            var radius = arc.Radius * zoom;
            var arcGeom = CreateArcGeometry(center, radius, arc.StartAngle, arc.EndAngle);
            context.DrawGeometry(null, pen, arcGeom);
        }

        private static Geometry CreateArcGeometry(Point center, double radius,
                                                   double startAngleDeg, double endAngleDeg)
        {
            if (endAngleDeg < startAngleDeg)
                endAngleDeg += 360.0;

            double sweepAngle = endAngleDeg - startAngleDeg;
            if (sweepAngle >= 360.0)
                return new EllipseGeometry(new Rect(center.X - radius, center.Y - radius, radius * 2, radius * 2));

            double startRad = startAngleDeg * Math.PI / 180.0;
            double endRad = endAngleDeg * Math.PI / 180.0;

            Point startPt = new Point(
                center.X + radius * Math.Cos(startRad),
                center.Y - radius * Math.Sin(startRad));

            Point endPt = new Point(
                center.X + radius * Math.Cos(endRad),
                center.Y - radius * Math.Sin(endRad));

            var pathGeometry = new PathGeometry();
            var pathFigure = new PathFigure { StartPoint = startPt, IsClosed = false };

            var arcSegment = new ArcSegment
            {
                Point = endPt,
                Size = new Size(radius, radius),
                RotationAngle = 0,
                IsLargeArc = sweepAngle > 180.0,
                SweepDirection = SweepDirection.CounterClockwise
            };

            pathFigure.Segments!.Add(arcSegment);
            pathGeometry.Figures!.Add(pathFigure);

            return pathGeometry;
        }
    }
}