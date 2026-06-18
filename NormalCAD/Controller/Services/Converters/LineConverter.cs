using ACadSharp;
using NormalCAD.Core.Entities;
using NormalCAD.Core.Geometry;
using AcadLine = ACadSharp.Entities.Line;
using CSMath;

namespace NormalCAD.Controller.Services.Converters
{
    public class LineConverter : EntityConverter<Line, AcadLine>
    {
        public override AcadLine ConvertToAcad(Line source, CadDocument cadDoc)
        {
            var result = new AcadLine(
                new XYZ(source.StartPoint.X, source.StartPoint.Y, source.StartPoint.Z),
                new XYZ(source.EndPoint.X, source.EndPoint.Y, source.EndPoint.Z)
            );
            ApplyLayerAndColorToAcad(result, source, cadDoc);
            return result;
        }

        public override Line ConvertToNormal(AcadLine source)
        {
            var result = new Line(
                new Point3d(source.StartPoint.X, source.StartPoint.Y, source.StartPoint.Z),
                new Point3d(source.EndPoint.X, source.EndPoint.Y, source.EndPoint.Z)
            );
            ApplyLayerAndColorToNormal(result, source);
            return result;
        }
    }
}
