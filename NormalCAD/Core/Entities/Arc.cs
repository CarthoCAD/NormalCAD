using NormalCAD.Core.Geometry;

namespace NormalCAD.Core.Entities
{
    public class Arc : Entity
    {
        public Point3d Center { get; set; }
        public double Radius { get; set; }
        public double StartAngle { get; set; } // Em graus (ACadSharp usa radianos, conversão feita no DxfService)
        public double EndAngle { get; set; } // Em graus

        public Arc()
        {
            Center = Point3d.Origin;
            Radius = 1.0;
            StartAngle = 0.0;
            EndAngle = 180.0;
        }

        public Arc(Point3d center, double radius, double startAngle, double endAngle)
        {
            Center = center;
            Radius = radius;
            StartAngle = startAngle;
            EndAngle = endAngle;
        }

        public override Entity Clone()
        {
            return new Arc(Center, Radius, StartAngle, EndAngle)
            {
                Layer = this.Layer,
                Color = this.Color
            };
        }
    }
}
