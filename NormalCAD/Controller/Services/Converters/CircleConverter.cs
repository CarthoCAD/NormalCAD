using ACadSharp;
using NormalCAD.Core.Entities;
using NormalCAD.Core.Geometry;
using AcadCircle = ACadSharp.Entities.Circle;
using CSMath;

namespace NormalCAD.Controller.Services.Converters
{
    public class CircleConverter : EntityConverter<Circle, AcadCircle>
    {
        public override AcadCircle ConvertToAcad(Circle source, CadDocument cadDoc)
        {
            var result = new AcadCircle(
                new XYZ(source.Center.X, source.Center.Y, source.Center.Z),
                source.Radius
            );
            ApplyLayerAndColorToAcad(result, source, cadDoc);
            return result;
        }

        public override Circle ConvertToNormal(AcadCircle source)
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
