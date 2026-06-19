using System;
using System.Collections.Generic;
using NormalCAD.Core.Geometry;

namespace NormalCAD.Core.Entities
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
            return new Circle(Center, Radius)
            {
                Layer = this.Layer,
                Color = this.Color
            };
        }

        public override void TransformBy(Matrix3d transform)
        {
            Center = transform.TransformPoint(Center);
            Radius = transform.TransformVector(new Vector3d(Radius, 0, 0)).Length;
        }

        public override IEnumerable<(Point3d Point, SnapType Type)> GetOsnapPoints()
        {
            yield return (Center, SnapType.Center);
        }
    }
}
