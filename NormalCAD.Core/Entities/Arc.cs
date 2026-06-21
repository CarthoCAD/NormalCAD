using System;
using System.Collections.Generic;
using NormalCAD.Core.Geometry;

namespace NormalCAD.Core.DatabaseServices
{
    public class Arc : Curve
    {
        public Point3d Center { get; set; }
        public double Radius { get; set; }
        public double StartAngle { get; set; }
        public double EndAngle { get; set; }

        public override Point3d StartPoint => PointAtAngle(StartAngle);
        public override Point3d EndPoint => PointAtAngle(EndAngle);
        public override double Length => Radius * Math.Abs(EndAngle - StartAngle) * Math.PI / 180.0;
        public override bool Closed => false;

        public override Extents3d GeometricExtents => ComputeExtents();

        public Arc()
        {
            Center = Point3d.Origin;
            Radius = 1.0;
            StartAngle = 0.0;
            EndAngle = 180.0;
        }

        public Arc(Point3d center, double radius, double startAngle, double endAngle)
        {
            Center = center;
            Radius = radius;
            StartAngle = startAngle;
            EndAngle = endAngle;
        }

        public override Entity Clone()
        {
            var clone = new Arc(Center, Radius, StartAngle, EndAngle);
            CopyEntityPropertiesTo(clone);
            return clone;
        }

        public override void TransformBy(Matrix3d transform)
        {
            Center = transform.TransformPoint(Center);
            Radius = transform.TransformVector(new Vector3d(Radius, 0, 0)).Length;
        }

        public override IEnumerable<(Point3d Point, SnapType Type)> GetOsnapPoints()
        {
            yield return (Center, SnapType.Center);
            yield return (PointAtAngle(StartAngle), SnapType.Endpoint);
            yield return (PointAtAngle(EndAngle), SnapType.Endpoint);
            double midAngle = (StartAngle + EndAngle) / 2.0;
            yield return (PointAtAngle(midAngle), SnapType.Midpoint);
        }

        public override IEnumerable<Point3d> GetGripPoints()
        {
            yield return Center;
            yield return PointAtAngle(StartAngle);
            yield return PointAtAngle(EndAngle);
            yield return PointAtAngle((StartAngle + EndAngle) / 2.0);
        }

        public override void MoveGripPointsAt(Point3dCollection grips, Vector3d offset)
        {
            if (grips.Count >= 1 && grips[0].DistanceTo(Center) < 1e-9)
            {
                Center = Center + offset;
                return;
            }

            var dir = grips[1] - Center;
            Radius = (dir + offset).Length;
        }

        public override IEnumerable<Point3d> GetStretchPoints()
        {
            yield return PointAtAngle(StartAngle);
            yield return PointAtAngle(EndAngle);
        }

        public override void MoveStretchPointsAt(Point3dCollection stretches, Vector3d offset)
        {
            foreach (var pt in stretches)
            {
                var dir = pt - Center;
                Radius = (dir + offset).Length;
            }
        }

        public override double GetDistanceTo(Point3d point)
        {
            double d = Center.DistanceTo(point);
            double distFromArc = Math.Abs(d - Radius);

            if (IsAngleOnArc(Math.Atan2(point.Y - Center.Y, point.X - Center.X) * 180.0 / Math.PI))
                return distFromArc;

            double d1 = StartPoint.DistanceTo(point);
            double d2 = EndPoint.DistanceTo(point);
            return Math.Min(Math.Min(d1, d2), distFromArc);
        }

        private bool IsAngleOnArc(double angleDeg)
        {
            if (angleDeg < 0) angleDeg += 360;
            double sa = StartAngle % 360;
            double ea = EndAngle % 360;
            if (sa < 0) sa += 360;
            if (ea < 0) ea += 360;

            if (sa <= ea)
                return angleDeg >= sa && angleDeg <= ea;
            else
                return angleDeg >= sa || angleDeg <= ea;
        }

        public override void IntersectWith(Entity entity, Intersect intersectType, Point3dCollection points)
        {
            switch (entity)
            {
                case Line line:
                {
                    var tempCircle = new Circle(Center, Radius);
                    var circlePoints = new Point3dCollection();
                    tempCircle.IntersectWith(line, intersectType, circlePoints);

                    foreach (var pt in circlePoints)
                    {
                        double angle = Math.Atan2(pt.Y - Center.Y, pt.X - Center.X) * 180.0 / Math.PI;
                        if (IsAngleOnArc(angle))
                            points.Add(pt);
                    }
                    break;
                }
                case Circle circle:
                    circle.IntersectWith(new Circle(Center, Radius), intersectType, points);
                    break;
                case Arc arc:
                    IntersectArcArc(this, arc, points);
                    break;
                case Polyline poly:
                    poly.IntersectWith(this, intersectType, points);
                    break;
            }
        }

        public override void List()
        {
            System.Diagnostics.Debug.WriteLine($"                  Arc");
            System.Diagnostics.Debug.WriteLine($"Layer: {Layer}");
            System.Diagnostics.Debug.WriteLine($"Center: ({Center.X:F4}, {Center.Y:F4}, {Center.Z:F4})");
            System.Diagnostics.Debug.WriteLine($"Radius: {Radius:F4}");
            System.Diagnostics.Debug.WriteLine($"Start Angle: {StartAngle:F4}");
            System.Diagnostics.Debug.WriteLine($"End Angle: {EndAngle:F4}");
            System.Diagnostics.Debug.WriteLine($"Length: {Length:F4}");
        }

        private static void IntersectArcArc(Arc a1, Arc a2, Point3dCollection points)
        {
            var c1 = new Circle(a1.Center, a1.Radius);
            var c2 = new Circle(a2.Center, a2.Radius);
            var circlePoints = new Point3dCollection();
            c1.IntersectWith(c2, Intersect.OnBothOperands, circlePoints);

            foreach (var pt in circlePoints)
            {
                double ang1 = Math.Atan2(pt.Y - a1.Center.Y, pt.X - a1.Center.X) * 180.0 / Math.PI;
                double ang2 = Math.Atan2(pt.Y - a2.Center.Y, pt.X - a2.Center.X) * 180.0 / Math.PI;
                if (a1.IsAngleOnArc(ang1) && a2.IsAngleOnArc(ang2))
                    points.Add(pt);
            }
        }

        private Point3d PointAtAngle(double angleDeg)
        {
            double rad = angleDeg * Math.PI / 180.0;
            return new Point3d(
                Center.X + Radius * Math.Cos(rad),
                Center.Y + Radius * Math.Sin(rad),
                Center.Z);
        }

        private Extents3d ComputeExtents()
        {
            double start = StartAngle, end = EndAngle;
            if (end < start) end += 360;

            double minX = double.MaxValue, minY = double.MaxValue;
            double maxX = double.MinValue, maxY = double.MinValue;

            int samples = 32;
            for (int i = 0; i <= samples; i++)
            {
                double a = start + (end - start) * i / samples;
                var pt = PointAtAngle(a);
                minX = Math.Min(minX, pt.X); maxX = Math.Max(maxX, pt.X);
                minY = Math.Min(minY, pt.Y); maxY = Math.Max(maxY, pt.Y);
            }

            return new Extents3d(new Point3d(minX, minY, 0), new Point3d(maxX, maxY, 0));
        }
    }
}
