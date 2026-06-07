using NormalCAD.Core.Geometry;

namespace NormalCAD.Core.Entities
{
    public class Line : Entity
    {
        public Point3d StartPoint { get; set; }
        public Point3d EndPoint { get; set; }

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
            return new Line(StartPoint, EndPoint)
            {
                Layer = this.Layer,
                Color = this.Color
            };
        }
    }
}
