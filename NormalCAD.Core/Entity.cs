using System.Collections.Generic;
using NormalCAD.Core.Geometry;

namespace NormalCAD.Core.DatabaseServices
{
    public abstract class Entity : DBObject
    {
        public string Layer { get; set; } = "0";
        public EntityColor Color { get; set; } = EntityColor.ByLayer;

        public abstract Entity Clone();
        public abstract Extents3d GeometricExtents { get; }

        public virtual IEnumerable<(Point3d Point, SnapType Type)> GetOsnapPoints()
        {
            yield break;
        }

        public abstract void TransformBy(Matrix3d transform);
    }
}
