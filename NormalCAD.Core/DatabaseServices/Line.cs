using System;
using System.Collections.Generic;
using NormalCAD.Core.Geometry;

namespace NormalCAD.Core.DatabaseServices
{
    public class Line : Curve
    {
        public new Point3d StartPoint { get; set; }
        public new Point3d EndPoint { get; set; }

        public double Thickness { get; set; }

        public Vector3d Delta => EndPoint - StartPoint;

        public double Angle => Math.Atan2(Delta.Y, Delta.X);

        public override double Length => StartPoint.DistanceTo(EndPoint);

        public override double Area => 0;

        public override bool Closed => false;

        
        public override Extents3d GeometricExtents =>
            Extents3d.FromPoints(StartPoint, EndPoint);

        public Line()
        {
            StartPoint = Point3d.Origin;
            EndPoint = Point3d.Origin;
        }

        public Line(Point3d start, Point3d end)
        {
            StartPoint = start;
            EndPoint = end;
        }

        public override Entity Clone()
        {
            var clone = new Line(StartPoint, EndPoint) { Thickness = Thickness };
            CopyEntityPropertiesTo(clone);
            return clone;
        }

        public override void TransformBy(Matrix3d transform)
        {
            StartPoint = transform.TransformPoint(StartPoint);
            EndPoint = transform.TransformPoint(EndPoint);
        }

        public override IEnumerable<(Point3d Point, SnapType Type)> GetOsnapPoints()
        {
            yield return (StartPoint, SnapType.Endpoint);
            yield return (EndPoint, SnapType.Endpoint);
            yield return (Midpoint, SnapType.Midpoint);
        }

        
        private Point3d Midpoint => new Point3d(
            (StartPoint.X + EndPoint.X) / 2,
            (StartPoint.Y + EndPoint.Y) / 2,
            (StartPoint.Z + EndPoint.Z) / 2);

        public override IEnumerable<Point3d> GetGripPoints()
        {
            yield return StartPoint;
            yield return Midpoint;
            yield return EndPoint;
        }

        public override void MoveGripPointsAt(Point3dCollection grips, Vector3d offset)
        {
            if (grips.Count >= 1 && grips[0].DistanceTo(StartPoint) < 1e-9)
            {
                StartPoint = StartPoint + offset;
                return;
            }
            if (grips.Count >= 2 && grips[1].DistanceTo(Midpoint) < 1e-9)
            {
                StartPoint = StartPoint + offset;
                EndPoint = EndPoint + offset;
                return;
            }
            if (grips.Count >= 3 && grips[2].DistanceTo(EndPoint) < 1e-9)
            {
                EndPoint = EndPoint + offset;
                return;
            }
        }

        public override IEnumerable<Point3d> GetStretchPoints()
        {
            yield return StartPoint;
            yield return EndPoint;
        }

        public override void MoveStretchPointsAt(Point3dCollection stretches, Vector3d offset)
        {
            if (stretches.Count >= 1 && stretches[0].DistanceTo(StartPoint) < 1e-9)
                StartPoint = StartPoint + offset;
            if (stretches.Count >= 2 && stretches[1].DistanceTo(EndPoint) < 1e-9)
                EndPoint = EndPoint + offset;
        }

        public override Curve3d? GetGeometricCurve()
            => new LineSegment3d(StartPoint, EndPoint);

        public override void List()
        {
            System.Diagnostics.Debug.WriteLine($"                  Line");
            System.Diagnostics.Debug.WriteLine($"Layer: {Layer}");
            System.Diagnostics.Debug.WriteLine($"From: ({StartPoint.X:F4}, {StartPoint.Y:F4}, {StartPoint.Z:F4})");
            System.Diagnostics.Debug.WriteLine($"To:   ({EndPoint.X:F4}, {EndPoint.Y:F4}, {EndPoint.Z:F4})");
            System.Diagnostics.Debug.WriteLine($"Length: {Length:F4}");
        }

    }
}
