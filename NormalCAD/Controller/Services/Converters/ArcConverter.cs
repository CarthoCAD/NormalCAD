using System;
using ACadSharp;
using NormalCAD.Core.Entities;
using NormalCAD.Core.Geometry;
using AcadArc = ACadSharp.Entities.Arc;
using CSMath;

namespace NormalCAD.Controller.Services.Converters
{
    public class ArcConverter : EntityConverter<Arc, AcadArc>
    {
        public override AcadArc ConvertToAcad(Arc source, CadDocument cadDoc)
        {
            // NormalCAD usa graus, ACadSharp usa radianos
            var result = new AcadArc(
                new XYZ(source.Center.X, source.Center.Y, source.Center.Z),
                source.Radius,
                source.StartAngle * Math.PI / 180.0,
                source.EndAngle * Math.PI / 180.0
            );
            ApplyLayerAndColorToAcad(result, source, cadDoc);
            return result;
        }

        public override Arc ConvertToNormal(AcadArc source)
        {
            // ACadSharp usa radianos, NormalCAD usa graus
            var result = new Arc(
                new Point3d(source.Center.X, source.Center.Y, source.Center.Z),
                source.Radius,
                source.StartAngle * 180.0 / Math.PI,
                source.EndAngle * 180.0 / Math.PI
            );
            ApplyLayerAndColorToNormal(result, source);
            return result;
        }
    }
}
