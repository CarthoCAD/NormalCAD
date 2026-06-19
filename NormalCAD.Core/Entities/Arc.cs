using System;
using System.Collections.Generic;
using NormalCAD.Core.Geometry;

namespace NormalCAD.Core.Entities
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
            return new Arc(Center, Radius, StartAngle, EndAngle)
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
            yield return (PointAtAngle(StartAngle), SnapType.Endpoint);
            yield return (PointAtAngle(EndAngle), SnapType.Endpoint);
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
