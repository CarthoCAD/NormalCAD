using ACadSharp;
using NormalCAD.Core.DatabaseServices;
using NormalCAD.Core.Geometry;
using CSMath;

namespace NormalCAD.Controller.Services.Converters
{
    public class ArcConverter : EntityConverter<Arc, ACadSharp.Entities.Arc>
    {
        public override ACadSharp.Entities.Arc ConvertToAcad(Arc source, CadDocument cadDoc)
        {
            var result = new ACadSharp.Entities.Arc(
                new XYZ(source.Center.X, source.Center.Y, source.Center.Z),
                source.Radius,
                source.StartAngle,
                source.EndAngle
            );
            ApplyEntityPropertiesToAcad(result, source, cadDoc);
            result.Normal = new XYZ(source.Normal.X, source.Normal.Y, source.Normal.Z);
            result.Thickness = source.Thickness;
            return result;
        }

        public override Arc ConvertToNormal(ACadSharp.Entities.Arc source)
        {
            var result = new Arc(
                new Point3d(source.Center.X, source.Center.Y, source.Center.Z),
                new Vector3d(source.Normal.X, source.Normal.Y, source.Normal.Z),
                source.Radius,
                source.StartAngle,
                source.EndAngle
            );
            ApplyEntityPropertiesToNormal(result, source);
            result.Thickness = source.Thickness;
            return result;
        }
    }
}
