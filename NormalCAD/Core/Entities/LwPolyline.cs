using System.Collections.Generic;
using NormalCAD.Core.Geometry;

namespace NormalCAD.Core.Entities
{
    public class LwPolyline : Entity
    {
        public List<Point3d> Vertices { get; set; } = new();
        public bool IsClosed { get; set; }

        public LwPolyline()
        {
        }

        public LwPolyline(IEnumerable<Point3d> vertices, bool isClosed = false)
        {
            Vertices = new List<Point3d>(vertices);
            IsClosed = isClosed;
        }

        public override Entity Clone()
        {
            return new LwPolyline(Vertices, IsClosed)
            {
                Layer = this.Layer,
                Color = this.Color
            };
        }
    }
}
