using System;
using System.Collections.Generic;
using NormalCAD.Core.Geometry;

namespace NormalCAD.Core.DatabaseServices
{
    public abstract class Curve : Entity
    {
        public abstract double Length { get; }
        public virtual bool Closed { get; set; }
        public virtual Point3d StartPoint => Point3d.Origin;

        public virtual Point3d EndPoint => Point3d.Origin;

        public override IEnumerable<Point3d> GetGripPoints()
        {
            var curve = GetGeometricCurve();
            if (curve != null)
            {
                yield return curve.StartPoint;
                yield return curve.EndPoint;
            }
        }

        public override IEnumerable<Point3d> GetStretchPoints()
        {
            var curve = GetGeometricCurve();
            if (curve != null)
            {
                yield return curve.StartPoint;
                yield return curve.EndPoint;
            }
        }

        public abstract double Area { get; }

        public virtual Point3d GetPointAtDist(double distance)
        {
            var curve = GetGeometricCurve();
            return curve?.GetPointAtDist(distance) ?? Point3d.Origin;
        }

        public virtual double GetDistAtPoint(Point3d point)
        {
            var curve = GetGeometricCurve();
            return curve?.GetDistAtPoint(point) ?? 0;
        }

        public virtual Point3d GetClosestPointTo(Point3d point, bool extend)
        {
            var curve = GetGeometricCurve();
            return curve?.GetClosestPointTo(point) ?? Point3d.Origin;
        }

        public virtual Point3d GetClosestPointTo(Point3d point)
        {
            return GetClosestPointTo(point, false);
        }

        public virtual double GetParameterAtDistance(double distance)
        {
            return distance;
        }

        public virtual double GetDistanceAtParameter(double parameter)
        {
            return parameter;
        }

        public virtual Point3d GetPointAtParameter(double parameter)
        {
            return GetPointAtDist(parameter);
        }

        public virtual double GetParameterAtPoint(Point3d point)
        {
            return GetDistAtPoint(point);
        }

        public virtual Vector3d GetFirstDerivative(Point3d point)
        {
            var curve = GetGeometricCurve();
            return curve?.GetFirstDerivative(point) ?? new Vector3d(0, 0, 0);
        }

        public virtual Vector3d GetFirstDerivative(double parameter)
        {
            var curve = GetGeometricCurve();
            return curve?.GetFirstDerivative(parameter) ?? new Vector3d(0, 0, 0);
        }
    }
}
