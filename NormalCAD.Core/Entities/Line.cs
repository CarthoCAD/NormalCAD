using System;
using System.Collections.Generic;
using System.ComponentModel;
using NormalCAD.Core.Geometry;

namespace NormalCAD.Core.DatabaseServices
{
    public class Line : Curve
    {
        private Point3d _startPoint;
        private Point3d _endPoint;

        public override Point3d StartPoint => _startPoint;
        public override Point3d EndPoint => _endPoint;

        [Category("Geometry")]
        [DisplayName("Start X")]
        public double StartX
        {
            get => _startPoint.X;
            set => _startPoint = new Point3d(value, _startPoint.Y, _startPoint.Z);
        }

        [Category("Geometry")]
        [DisplayName("Start Y")]
        public double StartY
        {
            get => _startPoint.Y;
            set => _startPoint = new Point3d(_startPoint.X, value, _startPoint.Z);
        }

        [Category("Geometry")]
        [DisplayName("Start Z")]
        public double StartZ
        {
            get => _startPoint.Z;
            set => _startPoint = new Point3d(_startPoint.X, _startPoint.Y, value);
        }

        [Category("Geometry")]
        [DisplayName("End X")]
        public double EndX
        {
            get => _endPoint.X;
            set => _endPoint = new Point3d(value, _endPoint.Y, _endPoint.Z);
        }

        [Category("Geometry")]
        [DisplayName("End Y")]
        public double EndY
        {
            get => _endPoint.Y;
            set => _endPoint = new Point3d(_endPoint.X, value, _endPoint.Z);
        }

        [Category("Geometry")]
        [DisplayName("End Z")]
        public double EndZ
        {
            get => _endPoint.Z;
            set => _endPoint = new Point3d(_endPoint.X, _endPoint.Y, value);
        }

        [Category("Geometry")]
        [DisplayName("Length")]
        [ReadOnly(true)]
        public override double Length => _startPoint.DistanceTo(_endPoint);

        [Category("Geometry")]
        [DisplayName("Closed")]
        [ReadOnly(true)]
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

        
        public Point3d Midpoint => new Point3d(
            (_startPoint.X + _endPoint.X) / 2,
            (_startPoint.Y + _endPoint.Y) / 2,
            (_startPoint.Z + _endPoint.Z) / 2);

        public override IEnumerable<Point3d> GetGripPoints()
        {
            yield return _startPoint;
            yield return Midpoint;
            yield return _endPoint;
        }

        public override void MoveGripPointsAt(Point3dCollection grips, Vector3d offset)
        {
            if (grips.Count >= 1 && grips[0].DistanceTo(_startPoint) < 1e-9)
            {
                _startPoint = _startPoint + offset;
                return;
            }
            if (grips.Count >= 2 && grips[1].DistanceTo(Midpoint) < 1e-9)
            {
                _startPoint = _startPoint + offset;
                _endPoint = _endPoint + offset;
                return;
            }
            if (grips.Count >= 3 && grips[2].DistanceTo(_endPoint) < 1e-9)
            {
                _endPoint = _endPoint + offset;
                return;
            }
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

        public override Curve3d? GetGeometricCurve()
            => new LineSegment3d(_startPoint, _endPoint);

        public override void List()
        {
            System.Diagnostics.Debug.WriteLine($"                  Line");
            System.Diagnostics.Debug.WriteLine($"Layer: {Layer}");
            System.Diagnostics.Debug.WriteLine($"From: ({_startPoint.X:F4}, {_startPoint.Y:F4}, {_startPoint.Z:F4})");
            System.Diagnostics.Debug.WriteLine($"To:   ({_endPoint.X:F4}, {_endPoint.Y:F4}, {_endPoint.Z:F4})");
            System.Diagnostics.Debug.WriteLine($"Length: {Length:F4}");
        }

    }
}
