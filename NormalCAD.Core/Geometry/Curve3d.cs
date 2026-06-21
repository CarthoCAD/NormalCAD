namespace NormalCAD.Core.Geometry
{
    public abstract class Curve3d
    {
        public abstract Point3d StartPoint { get; }
        public abstract Point3d EndPoint { get; }
        public abstract double Length { get; }

        public abstract void IntersectWith(Curve3d other, Point3dCollection points);
        public abstract double GetDistanceTo(Point3d point);

        public abstract Point3d GetPointAtDist(double distance);
        public abstract double GetDistAtPoint(Point3d point);
        public abstract Point3d GetClosestPointTo(Point3d point);

        public virtual Vector3d GetFirstDerivative(Point3d point)
        {
            var pt = GetClosestPointTo(point);
            return GetFirstDerivativeAt(GetDistAtPoint(pt));
        }

        public virtual Vector3d GetFirstDerivative(double distance)
        {
            return GetFirstDerivativeAt(distance);
        }

        protected abstract Vector3d GetFirstDerivativeAt(double distance);

        public virtual double GetAreaContribution()
        {
            return 0.5 * (StartPoint.X * EndPoint.Y - EndPoint.X * StartPoint.Y);
        }
    }
}
