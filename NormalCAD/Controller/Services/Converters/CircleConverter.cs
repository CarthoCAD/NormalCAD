using ACadSharp;
using NormalCAD.Core.DatabaseServices;
using NormalCAD.Core.Geometry;
using CSMath;

namespace NormalCAD.Controller.Services.Converters
{
    public class CircleConverter : EntityConverter<Circle, ACadSharp.Entities.Circle>
    {
        public override ACadSharp.Entities.Circle ConvertToAcad(Circle source, CadDocument cadDoc)
        {
            var result = new ACadSharp.Entities.Circle(
                new XYZ(source.Center.X, source.Center.Y, source.Center.Z),
                source.Radius
            );
            ApplyLayerAndColorToAcad(result, source, cadDoc);
            return result;
        }

        public override Circle ConvertToNormal(ACadSharp.Entities.Circle source)
        {
            var result = new Circle(
                new Point3d(source.Center.X, source.Center.Y, source.Center.Z),
                source.Radius
            );
            ApplyLayerAndColorToNormal(result, source);
            return result;
        }
    }
}
