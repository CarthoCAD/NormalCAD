using System;
using System.Collections.Generic;
using NormalCAD.Core.Geometry;

namespace NormalCAD.Core.DatabaseServices
{
    public class Circle : Curve
    {
        public Point3d Center { get; set; }

        public double Radius { get; set; }

        public double Diameter
        {
            get => Radius * 2.0;
            set => Radius = value / 2.0;
        }

        public double Circumference
        {
            get => 2 * Math.PI * Radius;
            set => Radius = value / (2 * Math.PI);
        }

        public Vector3d Normal { get; set; } = Vector3d.ZAxis;

        
        public override Point3d StartPoint => new Point3d(Center.X + Radius, Center.Y, Center.Z);

        
        public override Point3d EndPoint => StartPoint;

        public override double Length => 2 * Math.PI * Radius;

        public override double Area => Math.PI * Radius * Radius;

        public override bool Closed => true;

        public override Extents3d GeometricExtents => new Extents3d(
            new Point3d(Center.X - Radius, Center.Y - Radius, 0),
            new Point3d(Center.X + Radius, Center.Y + Radius, 0));

        public Circle()
        {
            Center = Point3d.Origin;
            Radius = 1.0;
        }

        public Circle(Point3d center, Vector3d normal, double radius)
        {
            Center = center;
            Normal = normal;
            Radius = radius;
        }

        public override Entity Clone()
        {
            var clone = new Circle(Center, Normal, Radius);
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

        public override Curve3d? GetGeometricCurve()
            => CircularArc3d.FullCircle(Center, Radius);

        public override void List()
        {
            System.Diagnostics.Debug.WriteLine($"                  Circle");
            System.Diagnostics.Debug.WriteLine($"Layer: {Layer}");
            System.Diagnostics.Debug.WriteLine($"Center: ({Center.X:F4}, {Center.Y:F4}, {Center.Z:F4})");
            System.Diagnostics.Debug.WriteLine($"Radius: {Radius:F4}");
            System.Diagnostics.Debug.WriteLine($"Length: {Length:F4}");
        }
    }
}
