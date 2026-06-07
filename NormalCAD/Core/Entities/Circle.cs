using NormalCAD.Core.Geometry;

namespace NormalCAD.Core.Entities
{
    public class Circle : Entity
    {
        public Point3d Center { get; set; }
        public double Radius { get; set; }

        public Circle()
        {
            Center = Point3d.Origin;
            Radius = 1.0;
        }

        public Circle(Point3d center, double radius)
        {
            Center = center;
            Radius = radius;
        }

        public override Entity Clone()
        {
            return new Circle(Center, Radius)
            {
                Layer = this.Layer,
                Color = this.Color
            };
        }
    }
}
