using System;
using System.Collections.Generic;
using NormalCAD.Core.Geometry;

namespace NormalCAD.Core.DatabaseServices
{
    public class Line : Curve
    {
        private Point3d _startPoint;
        private Point3d _endPoint;

        public override Point3d StartPoint => _startPoint;
        public override Point3d EndPoint => _endPoint;
        public override double Length => _startPoint.DistanceTo(_endPoint);
        public override bool Closed => false;

        public override Extents3d GeometricExtents =>
            Extents3d.FromPoints(_startPoint, _endPoint);

        public Line()
        {
            _startPoint = Point3d.Origin;
            _endPoint = Point3d.Origin;
        }

        public Line(Point3d start, Point3d end)
        {
            _startPoint = start;
            _endPoint = end;
        }

        public override Entity Clone()
        {
            var clone = new Line(_startPoint, _endPoint);
            CopyEntityPropertiesTo(clone);
            return clone;
        }

        public override void TransformBy(Matrix3d transform)
        {
            _startPoint = transform.TransformPoint(_startPoint);
            _endPoint = transform.TransformPoint(_endPoint);
        }

        public override IEnumerable<(Point3d Point, SnapType Type)> GetOsnapPoints()
        {
            yield return (_startPoint, SnapType.Endpoint);
            yield return (_endPoint, SnapType.Endpoint);
            yield return (new Point3d(
                (_startPoint.X + _endPoint.X) / 2,
                (_startPoint.Y + _endPoint.Y) / 2,
                (_startPoint.Z + _endPoint.Z) / 2), SnapType.Midpoint);
        }

        public override IEnumerable<Point3d> GetGripPoints()
        {
            yield return _startPoint;
            yield return _endPoint;
        }

        public override void MoveGripPointsAt(Point3dCollection grips, Vector3d offset)
        {
            if (grips.Count >= 1 && grips[0].DistanceTo(_startPoint) < 1e-9)
                _startPoint = _startPoint + offset;
            if (grips.Count >= 2 && grips[1].DistanceTo(_endPoint) < 1e-9)
                _endPoint = _endPoint + offset;
        }

        public override IEnumerable<Point3d> GetStretchPoints()
        {
            yield return _startPoint;
            yield return _endPoint;
        }

        public override void MoveStretchPointsAt(Point3dCollection stretches, Vector3d offset)
        {
            if (stretches.Count >= 1 && stretches[0].DistanceTo(_startPoint) < 1e-9)
                _startPoint = _startPoint + offset;
            if (stretches.Count >= 2 && stretches[1].DistanceTo(_endPoint) < 1e-9)
                _endPoint = _endPoint + offset;
        }

        public override double GetDistanceTo(Point3d point)
        {
            double dx = _endPoint.X - _startPoint.X;
            double dy = _endPoint.Y - _startPoint.Y;
            double lenSq = dx * dx + dy * dy;
            if (lenSq < 1e-12)
                return _startPoint.DistanceTo(point);

            double t = ((point.X - _startPoint.X) * dx + (point.Y - _startPoint.Y) * dy) / lenSq;
            t = Math.Max(0, Math.Min(1, t));

            double projX = _startPoint.X + t * dx;
            double projY = _startPoint.Y + t * dy;
            return new Point3d(projX, projY, _startPoint.Z).DistanceTo(point);
        }

        public override void IntersectWith(Entity entity, Intersect intersectType, Point3dCollection points)
        {
            switch (entity)
            {
                case Line line:
                    IntersectLineLine(this, line, points);
                    break;
                case Circle circle:
                    IntersectLineCircle(this, circle, points);
                    break;
                case Arc arc:
                    IntersectLineArc(this, arc, points);
                    break;
                case Polyline poly:
                    poly.IntersectWith(this, intersectType, points);
                    break;
            }
        }

        public override void List()
        {
            System.Diagnostics.Debug.WriteLine($"                  Line");
            System.Diagnostics.Debug.WriteLine($"Layer: {Layer}");
            System.Diagnostics.Debug.WriteLine($"From: ({_startPoint.X:F4}, {_startPoint.Y:F4}, {_startPoint.Z:F4})");
            System.Diagnostics.Debug.WriteLine($"To:   ({_endPoint.X:F4}, {_endPoint.Y:F4}, {_endPoint.Z:F4})");
            System.Diagnostics.Debug.WriteLine($"Length: {Length:F4}");
        }

        private static void IntersectLineLine(Line l1, Line l2, Point3dCollection points)
        {
            double x1 = l1._startPoint.X, y1 = l1._startPoint.Y;
            double x2 = l1._endPoint.X, y2 = l1._endPoint.Y;
            double x3 = l2._startPoint.X, y3 = l2._startPoint.Y;
            double x4 = l2._endPoint.X, y4 = l2._endPoint.Y;

            double denom = (x1 - x2) * (y3 - y4) - (y1 - y2) * (x3 - x4);
            if (Math.Abs(denom) < 1e-12) return;

            double t = ((x1 - x3) * (y3 - y4) - (y1 - y3) * (x3 - x4)) / denom;
            double u = -((x1 - x2) * (y1 - y3) - (y1 - y2) * (x1 - x3)) / denom;

            if (t >= 0 && t <= 1 && u >= 0 && u <= 1)
            {
                points.Add(new Point3d(x1 + t * (x2 - x1), y1 + t * (y2 - y1), 0));
            }
        }

        private static void IntersectLineCircle(Line line, Circle circle, Point3dCollection points)
        {
            double cx = circle.Center.X, cy = circle.Center.Y, r = circle.Radius;
            double x1 = line._startPoint.X, y1 = line._startPoint.Y;
            double x2 = line._endPoint.X, y2 = line._endPoint.Y;

            double dx = x2 - x1;
            double dy = y2 - y1;
            double fx = x1 - cx;
            double fy = y1 - cy;

            double a = dx * dx + dy * dy;
            double b = 2 * (fx * dx + fy * dy);
            double c = fx * fx + fy * fy - r * r;

            double discriminant = b * b - 4 * a * c;
            if (discriminant < 0) return;

            double sqrtD = Math.Sqrt(discriminant);
            double t1 = (-b - sqrtD) / (2 * a);
            double t2 = (-b + sqrtD) / (2 * a);

            if (t1 >= 0 && t1 <= 1)
                points.Add(new Point3d(x1 + t1 * dx, y1 + t1 * dy, 0));
            if (t2 >= 0 && t2 <= 1 && Math.Abs(t1 - t2) > 1e-9)
                points.Add(new Point3d(x1 + t2 * dx, y1 + t2 * dy, 0));
        }

        private static void IntersectLineArc(Line line, Arc arc, Point3dCollection points)
        {
            var tempCircle = new Circle(arc.Center, arc.Radius);
            var circlePoints = new Point3dCollection();
            IntersectLineCircle(line, tempCircle, circlePoints);

            foreach (var pt in circlePoints)
            {
                double angle = Math.Atan2(pt.Y - arc.Center.Y, pt.X - arc.Center.X) * 180.0 / Math.PI;
                if (angle < 0) angle += 360;

                double sa = arc.StartAngle % 360;
                double ea = arc.EndAngle % 360;
                if (sa < 0) sa += 360;
                if (ea < 0) ea += 360;

                if (sa <= ea)
                {
                    if (angle >= sa && angle <= ea)
                        points.Add(pt);
                }
                else
                {
                    if (angle >= sa || angle <= ea)
                        points.Add(pt);
                }
            }
        }
    }
}
