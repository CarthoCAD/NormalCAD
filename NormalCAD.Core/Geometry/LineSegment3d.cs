using System;

namespace NormalCAD.Core.Geometry
{
    public class LineSegment3d : Curve3d
    {
        public Point3d P0 { get; }
        public Point3d P1 { get; }

        public override Point3d StartPoint => P0;
        public override Point3d EndPoint => P1;
        public override double Length => P0.DistanceTo(P1);

        public LineSegment3d(Point3d p0, Point3d p1)
        {
            P0 = p0;
            P1 = p1;
        }

        public override Point3d GetPointAtDist(double distance)
        {
            double len = Length;
            if (len < 1e-12) return P0;
            double t = Math.Max(0, Math.Min(1, distance / len));
            return new Point3d(
                P0.X + t * (P1.X - P0.X),
                P0.Y + t * (P1.Y - P0.Y),
                P0.Z + t * (P1.Z - P0.Z));
        }

        public override double GetDistAtPoint(Point3d point)
        {
            double dx = P1.X - P0.X;
            double dy = P1.Y - P0.Y;
            double len = Length;
            if (len < 1e-12) return 0;
            double t = ((point.X - P0.X) * dx + (point.Y - P0.Y) * dy) / (len * len);
            t = Math.Max(0, Math.Min(1, t));
            return t * len;
        }

        public override Point3d GetClosestPointTo(Point3d point)
        {
            double dx = P1.X - P0.X;
            double dy = P1.Y - P0.Y;
            double lenSq = dx * dx + dy * dy;
            if (lenSq < 1e-12) return P0;

            double t = ((point.X - P0.X) * dx + (point.Y - P0.Y) * dy) / lenSq;
            t = Math.Max(0, Math.Min(1, t));
            return new Point3d(
                P0.X + t * dx,
                P0.Y + t * dy,
                P0.Z + t * (P1.Z - P0.Z));
        }

        public override double GetDistanceTo(Point3d point)
        {
            return GetClosestPointTo(point).DistanceTo(point);
        }

        protected override Vector3d GetFirstDerivativeAt(double distance)
        {
            double len = Length;
            if (len < 1e-12) return new Vector3d(0, 0, 0);
            return new Vector3d(
                (P1.X - P0.X) / len,
                (P1.Y - P0.Y) / len,
                (P1.Z - P0.Z) / len);
        }

        public override void IntersectWith(Curve3d other, Point3dCollection points)
        {
            switch (other)
            {
                case LineSegment3d seg:
                    IntersectLineLine(seg, points);
                    break;
                case CircularArc3d arc:
                    IntersectLineArc(arc, points);
                    break;
                case CompositeCurve3d comp:
                    comp.IntersectWith(this, points);
                    break;
            }
        }

        private void IntersectLineLine(LineSegment3d other, Point3dCollection points)
        {
            double x1 = P0.X, y1 = P0.Y;
            double x2 = P1.X, y2 = P1.Y;
            double x3 = other.P0.X, y3 = other.P0.Y;
            double x4 = other.P1.X, y4 = other.P1.Y;

            double denom = (x1 - x2) * (y3 - y4) - (y1 - y2) * (x3 - x4);
            if (Math.Abs(denom) < 1e-12) return;

            double t = ((x1 - x3) * (y3 - y4) - (y1 - y3) * (x3 - x4)) / denom;
            double u = -((x1 - x2) * (y1 - y3) - (y1 - y2) * (x1 - x3)) / denom;

            if (t >= 0 && t <= 1 && u >= 0 && u <= 1)
                points.Add(new Point3d(x1 + t * (x2 - x1), y1 + t * (y2 - y1), 0));
        }

        private void IntersectLineArc(CircularArc3d arc, Point3dCollection points)
        {
            double cx = arc.Center.X, cy = arc.Center.Y, r = arc.Radius;
            double x1 = P0.X, y1 = P0.Y;
            double x2 = P1.X, y2 = P1.Y;

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
            {
                var pt = new Point3d(x1 + t1 * dx, y1 + t1 * dy, 0);
                if (arc.IsPointOnArc(pt))
                    points.Add(pt);
            }
            if (t2 >= 0 && t2 <= 1 && Math.Abs(t1 - t2) > 1e-9)
            {
                var pt = new Point3d(x1 + t2 * dx, y1 + t2 * dy, 0);
                if (arc.IsPointOnArc(pt))
                    points.Add(pt);
            }
        }
    }
}
