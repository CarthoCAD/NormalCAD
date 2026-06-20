using NormalCAD.Core.Geometry;

namespace NormalCAD.Core.DatabaseServices
{
    public abstract class Curve : Entity
    {
        public abstract double Length { get; }
        public virtual bool Closed => false;
        public abstract Point3d StartPoint { get; }
        public abstract Point3d EndPoint { get; }
    }
}
