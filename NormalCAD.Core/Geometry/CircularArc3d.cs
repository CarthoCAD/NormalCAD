using System;

namespace NormalCAD.Core.Geometry
{
    public class CircularArc3d : Curve3d
    {
        public Point3d Center { get; }
        public double Radius { get; }
        public double StartAngle { get; }
        public double EndAngle { get; }

        public double SweepAngle => EndAngle - StartAngle;

        public override Point3d StartPoint => PointAtAngle(StartAngle);
        public override Point3d EndPoint => PointAtAngle(EndAngle);
        public override double Length => Radius * Math.Abs(SweepAngle);

        public CircularArc3d(Point3d center, double radius, double startAngle, double endAngle)
        {
            Center = center;
            Radius = radius;
            StartAngle = startAngle;
            EndAngle = endAngle;
        }

        public static CircularArc3d FullCircle(Point3d center, double radius)
            => new CircularArc3d(center, radius, 0, 2 * Math.PI);

        public bool IsFullCircle => Math.Abs(SweepAngle) >= 2 * Math.PI ||
                                    Math.Abs(Math.Abs(SweepAngle) % (2 * Math.PI)) < 1e-9;

        public bool IsPointOnArc(Point3d pt)
        {
            double d = Center.DistanceTo(pt);
            if (Math.Abs(d - Radius) > 1e-9)
                return false;

            if (IsFullCircle)
                return true;

            double angle = Math.Atan2(pt.Y - Center.Y, pt.X - Center.X);
            if (angle < 0) angle += 2 * Math.PI;
            return IsAngleOnArc(angle);
        }

        private bool IsAngleOnArc(double angle)
        {
            double sa = StartAngle % (2 * Math.PI);
            double ea = EndAngle % (2 * Math.PI);
            if (sa < 0) sa += 2 * Math.PI;
            if (ea < 0) ea += 2 * Math.PI;

            if (sa <= ea)
                return angle >= sa - 1e-9 && angle <= ea + 1e-9;
            else
                return angle >= sa - 1e-9 || angle <= ea + 1e-9;
        }

        public Point3d PointAtAngle(double angle)
        {
            return new Point3d(
                Center.X + Radius * Math.Cos(angle),
                Center.Y + Radius * Math.Sin(angle),
                Center.Z);
        }

        public override Point3d GetPointAtDist(double distance)
        {
            double len = Length;
            if (len < 1e-12) return PointAtAngle(StartAngle);
            double t = distance / len;
            double angle = StartAngle + t * SweepAngle;
            return PointAtAngle(angle);
        }

        public override double GetDistAtPoint(Point3d point)
        {
            double angle = Math.Atan2(point.Y - Center.Y, point.X - Center.X);

            double distFromStart = (angle - StartAngle) * Radius;
            if (SweepAngle < 0)
                distFromStart = -distFromStart;
            if (distFromStart < 0)
                distFromStart += Length;

            return Math.Max(0, Math.Min(Length, distFromStart));
        }

        public override Point3d GetClosestPointTo(Point3d point)
        {
            double dx = point.X - Center.X;
            double dy = point.Y - Center.Y;
            double d = Math.Sqrt(dx * dx + dy * dy);

            if (d < 1e-12)
            {
                if (IsFullCircle)
                    return new Point3d(Center.X + Radius, Center.Y, Center.Z);
                return StartPoint;
            }

            var ptOnCircle = new Point3d(
                Center.X + dx * Radius / d,
                Center.Y + dy * Radius / d,
                Center.Z);

            if (IsFullCircle || IsPointOnArc(ptOnCircle))
                return ptOnCircle;

            double d1 = StartPoint.DistanceTo(point);
            double d2 = EndPoint.DistanceTo(point);
            return d1 <= d2 ? StartPoint : EndPoint;
        }

        public override double GetDistanceTo(Point3d point)
        {
            return GetClosestPointTo(point).DistanceTo(point);
        }

        public override double GetAreaContribution()
        {
            double Δθ = SweepAngle;
            double cx = Center.X, cy = Center.Y;
            double x1 = StartPoint.X, y1 = StartPoint.Y;
            double x2 = EndPoint.X, y2 = EndPoint.Y;
            return 0.5 * (cx * (y2 - y1) - cy * (x2 - x1) + Radius * Radius * Δθ);
        }

        protected override Vector3d GetFirstDerivativeAt(double distance)
        {
            var pt = GetPointAtDist(distance);
            double dx = -(pt.Y - Center.Y);
            double dy = pt.X - Center.X;
            double len = Math.Sqrt(dx * dx + dy * dy);
            if (len < 1e-12) return new Vector3d(0, 0, 0);
            double sign = SweepAngle >= 0 ? 1 : -1;
            return new Vector3d(sign * dx / len, sign * dy / len, 0);
        }

        public override void IntersectWith(Curve3d other, Point3dCollection points)
        {
            switch (other)
            {
                case LineSegment3d seg:
                    seg.IntersectWith(this, points);
                    break;
                case CircularArc3d arc:
                    IntersectArcArc(arc, points);
                    break;
                case CompositeCurve3d comp:
                    comp.IntersectWith(this, points);
                    break;
            }
        }

        private void IntersectArcArc(CircularArc3d other, Point3dCollection points)
        {
            double d = Center.DistanceTo(other.Center);
            double r1 = Radius, r2 = other.Radius;

            if (d > r1 + r2 + 1e-9 || d < Math.Abs(r1 - r2) - 1e-9 || d < 1e-12)
                return;

            double a = (r1 * r1 - r2 * r2 + d * d) / (2 * d);
            double hSq = r1 * r1 - a * a;
            if (hSq < -1e-9) return;

            double h = hSq > 0 ? Math.Sqrt(hSq) : 0;
            double cx2 = Center.X + a * (other.Center.X - Center.X) / d;
            double cy2 = Center.Y + a * (other.Center.Y - Center.Y) / d;

            var p1 = new Point3d(
                cx2 + h * (other.Center.Y - Center.Y) / d,
                cy2 - h * (other.Center.X - Center.X) / d, 0);

            if (IsPointOnArc(p1) && other.IsPointOnArc(p1))
                points.Add(p1);

            if (h > 1e-9)
            {
                var p2 = new Point3d(
                    cx2 - h * (other.Center.Y - Center.Y) / d,
                    cy2 + h * (other.Center.X - Center.X) / d, 0);
                if (IsPointOnArc(p2) && other.IsPointOnArc(p2))
                    points.Add(p2);
            }
        }
    }
}
