using System.Collections.Generic;
using NormalCAD.Core.Geometry;

namespace NormalCAD.Core.Entities
{
    public class Line : Curve
    {
        private Point3d _startPoint;
        private Point3d _endPoint;

        public override Point3d StartPoint => _startPoint;
        public override Point3d EndPoint => _endPoint;
        public override double Length => _startPoint.DistanceTo(_endPoint);
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
            return new Line(_startPoint, _endPoint)
            {
                Layer = this.Layer,
                Color = this.Color
            };
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
    }
}
