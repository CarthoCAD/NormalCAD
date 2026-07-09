using ACadSharp;
using NormalCAD.Core.DatabaseServices;
using NormalCAD.Core.Geometry;
using CSMath;

namespace NormalCAD.Controller.Services.Converters
{
    public class LineConverter : EntityConverter<Line, ACadSharp.Entities.Line>
    {
        public override ACadSharp.Entities.Line ConvertToAcad(Line source, CadDocument cadDoc)
        {
            var result = new ACadSharp.Entities.Line(
                new XYZ(source.StartPoint.X, source.StartPoint.Y, source.StartPoint.Z),
                new XYZ(source.EndPoint.X, source.EndPoint.Y, source.EndPoint.Z)
            );
            ApplyEntityPropertiesToAcad(result, source, cadDoc);
            result.Thickness = source.Thickness;
            return result;
        }

        public override Line ConvertToNormal(ACadSharp.Entities.Line source)
        {
            var result = new Line(
                new Point3d(source.StartPoint.X, source.StartPoint.Y, source.StartPoint.Z),
                new Point3d(source.EndPoint.X, source.EndPoint.Y, source.EndPoint.Z)
            );
            ApplyEntityPropertiesToNormal(result, source);
            result.Thickness = source.Thickness;
            return result;
        }
    }
}
