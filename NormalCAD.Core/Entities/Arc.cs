using System;
using System.Collections.Generic;
using System.ComponentModel;
using NormalCAD.Core.Geometry;

namespace NormalCAD.Core.DatabaseServices
{
    public class Arc : Curve
    {
        [Category("Geometry")]
        [DisplayName("Center X")]
        public double CenterX
        {
            get => Center.X;
            set => Center = new Point3d(value, Center.Y, Center.Z);
        }

        [Category("Geometry")]
        [DisplayName("Center Y")]
        public double CenterY
        {
            get => Center.Y;
            set => Center = new Point3d(Center.X, value, Center.Z);
        }

        [Category("Geometry")]
        [DisplayName("Radius")]
        public double Radius { get; set; }

        [Category("Geometry")]
        [DisplayName("Start Angle")]
        public double StartAngle { get; set; }

        [Category("Geometry")]
        [DisplayName("End Angle")]
        public double EndAngle { get; set; }

        
        public Point3d Center { get; set; }

        
        public override Point3d StartPoint => PointAtAngle(StartAngle);

        
        public override Point3d EndPoint => PointAtAngle(EndAngle);

        [Category("Geometry")]
        [DisplayName("Length")]
        [ReadOnly(true)]
        public override double Length => Radius * Math.Abs(EndAngle - StartAngle) * Math.PI / 180.0;

        [Category("Geometry")]
        [DisplayName("Closed")]
        [ReadOnly(true)]
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

        public override Curve3d? GetGeometricCurve()
            => new CircularArc3d(Center, Radius, StartAngle, EndAngle);

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
