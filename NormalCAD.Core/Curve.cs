using System.Collections.Generic;
using NormalCAD.Core.Geometry;

namespace NormalCAD.Core.DatabaseServices
{
    public abstract class Curve : Entity
    {
        public abstract double Length { get; }
        public virtual bool Closed => false;
        public abstract Point3d StartPoint { get; }
        public abstract Point3d EndPoint { get; }

        public override IEnumerable<Point3d> GetGripPoints()
        {
            yield return StartPoint;
            yield return EndPoint;
        }

        public override IEnumerable<Point3d> GetStretchPoints()
        {
            yield return StartPoint;
            yield return EndPoint;
        }

        public override double GetDistanceTo(Point3d point)
        {
            return StartPoint.DistanceTo(point) < EndPoint.DistanceTo(point)
                ? StartPoint.DistanceTo(point)
                : EndPoint.DistanceTo(point);
        }
    }
}
