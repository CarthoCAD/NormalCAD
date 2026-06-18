using System.Collections.Generic;
using ACadSharp;
using NormalCAD.Core.Entities;
using NormalCAD.Core.Geometry;
using CSMath;

namespace NormalCAD.Controller.Services.Converters
{
    public class LwPolylineConverter : EntityConverter<LwPolyline, ACadSharp.Entities.LwPolyline>
    {
        public override ACadSharp.Entities.LwPolyline ConvertToAcad(LwPolyline source, CadDocument cadDoc)
        {
            var vertices = new List<ACadSharp.Entities.LwPolyline.Vertex>();
            foreach (var pt in source.Vertices)
                vertices.Add(new ACadSharp.Entities.LwPolyline.Vertex(new XY(pt.X, pt.Y)));

            var result = new ACadSharp.Entities.LwPolyline(vertices)
            {
                IsClosed = source.IsClosed
            };
            ApplyLayerAndColorToAcad(result, source, cadDoc);
            return result;
        }

        public override LwPolyline ConvertToNormal(ACadSharp.Entities.LwPolyline source)
        {
            var vertices = new List<Point3d>();
            foreach (var v in source.Vertices)
                vertices.Add(new Point3d(v.Location.X, v.Location.Y, 0));

            var result = new LwPolyline(vertices, source.IsClosed);
            ApplyLayerAndColorToNormal(result, source);
            return result;
        }
    }
}
