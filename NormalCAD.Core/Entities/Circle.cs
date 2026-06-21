using System;
using System.Collections.Generic;
using NormalCAD.Core.Geometry;

namespace NormalCAD.Core.DatabaseServices
{
    public class Circle : Curve
    {
        public Point3d Center { get; set; }
        public double Radius { get; set; }

        public override Point3d StartPoint => new Point3d(Center.X + Radius, Center.Y, Center.Z);
        public override Point3d EndPoint => StartPoint;
        public override double Length => 2 * Math.PI * Radius;
        public override bool Closed => true;

        public override Extents3d GeometricExtents => new Extents3d(
            new Point3d(Center.X - Radius, Center.Y - Radius, 0),
            new Point3d(Center.X + Radius, Center.Y + Radius, 0));

        public Circle()
        {
            Center = Point3d.Origin;
            Radius = 1.0;
        }

        public Circle(Point3d center, double radius)
        {
            Center = center;
            Radius = radius;
        }

        public override Entity Clone()
        {
            var clone = new Circle(Center, Radius);
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
            yield return (new Point3d(Center.X + Radius, Center.Y, Center.Z), SnapType.Endpoint);
            yield return (new Point3d(Center.X - Radius, Center.Y, Center.Z), SnapType.Endpoint);
            yield return (new Point3d(Center.X, Center.Y + Radius, Center.Z), SnapType.Endpoint);
            yield return (new Point3d(Center.X, Center.Y - Radius, Center.Z), SnapType.Endpoint);
        }

        public override IEnumerable<Point3d> GetGripPoints()
        {
            yield return Center;
            yield return new Point3d(Center.X + Radius, Center.Y, Center.Z);
            yield return new Point3d(Center.X - Radius, Center.Y, Center.Z);
            yield return new Point3d(Center.X, Center.Y + Radius, Center.Z);
            yield return new Point3d(Center.X, Center.Y - Radius, Center.Z);
        }

        public override void MoveGripPointsAt(Point3dCollection grips, Vector3d offset)
        {
            if (grips.Count >= 1 && grips[0].DistanceTo(Center) < 1e-9)
            {
                Center = Center + offset;
                return;
            }

            for (int i = 1; i < grips.Count; i++)
            {
                if (grips[i].DistanceTo(Center) > 1e-9)
                {
                    var dir = grips[i] - Center;
                    Radius = (dir + offset).Length;
                }
            }
        }

        public override IEnumerable<Point3d> GetStretchPoints()
        {
            yield return new Point3d(Center.X + Radius, Center.Y, Center.Z);
            yield return new Point3d(Center.X - Radius, Center.Y, Center.Z);
            yield return new Point3d(Center.X, Center.Y + Radius, Center.Z);
            yield return new Point3d(Center.X, Center.Y - Radius, Center.Z);
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
            return Math.Abs(d - Radius);
        }

        public override void IntersectWith(Entity entity, Intersect intersectType, Point3dCollection points)
        {
            switch (entity)
            {
                case Circle circle:
                    IntersectCircleCircle(this, circle, points);
                    break;
                case Line line:
                    line.IntersectWith(this, intersectType, points);
                    break;
                case Arc arc:
                    arc.IntersectWith(this, intersectType, points);
                    break;
                case Polyline poly:
                    poly.IntersectWith(this, intersectType, points);
                    break;
            }
        }

        public override void List()
        {
            System.Diagnostics.Debug.WriteLine($"                  Circle");
            System.Diagnostics.Debug.WriteLine($"Layer: {Layer}");
            System.Diagnostics.Debug.WriteLine($"Center: ({Center.X:F4}, {Center.Y:F4}, {Center.Z:F4})");
            System.Diagnostics.Debug.WriteLine($"Radius: {Radius:F4}");
            System.Diagnostics.Debug.WriteLine($"Length: {Length:F4}");
        }

        private static void IntersectCircleCircle(Circle c1, Circle c2, Point3dCollection points)
        {
            double d = c1.Center.DistanceTo(c2.Center);
            if (d > c1.Radius + c2.Radius || d < Math.Abs(c1.Radius - c2.Radius) || d < 1e-12)
                return;

            double a = (c1.Radius * c1.Radius - c2.Radius * c2.Radius + d * d) / (2 * d);
            double hSq = c1.Radius * c1.Radius - a * a;
            if (hSq < 0) return;

            double h = Math.Sqrt(hSq);
            double cx2 = c1.Center.X + a * (c2.Center.X - c1.Center.X) / d;
            double cy2 = c1.Center.Y + a * (c2.Center.Y - c1.Center.Y) / d;

            points.Add(new Point3d(
                cx2 + h * (c2.Center.Y - c1.Center.Y) / d,
                cy2 - h * (c2.Center.X - c1.Center.X) / d, 0));

            if (h > 1e-9)
                points.Add(new Point3d(
                    cx2 - h * (c2.Center.Y - c1.Center.Y) / d,
                    cy2 + h * (c2.Center.X - c1.Center.X) / d, 0));
        }
    }
}
