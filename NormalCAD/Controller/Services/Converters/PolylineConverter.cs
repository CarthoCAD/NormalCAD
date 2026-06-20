using System.Collections.Generic;
using ACadSharp;
using NormalCAD.Core.DatabaseServices;
using NormalCAD.Core.Geometry;
using CSMath;

namespace NormalCAD.Controller.Services.Converters
{
    public class PolylineConverter : EntityConverter<Polyline, ACadSharp.Entities.LwPolyline>
    {
        public override ACadSharp.Entities.LwPolyline ConvertToAcad(Polyline source, CadDocument cadDoc)
        {
            var vertices = new List<ACadSharp.Entities.LwPolyline.Vertex>();
            for (int i = 0; i < source.NumberOfVertices; i++)
            {
                var pt3d = source.GetPoint3dAt(i);
                vertices.Add(new ACadSharp.Entities.LwPolyline.Vertex(new XY(pt3d.X, pt3d.Y)));
            }

            var result = new ACadSharp.Entities.LwPolyline(vertices)
            {
                IsClosed = source.Closed,
                Elevation = source.Elevation
            };
            ApplyLayerAndColorToAcad(result, source, cadDoc);
            return result;
        }

        public override Polyline ConvertToNormal(ACadSharp.Entities.LwPolyline source)
        {
            var vertices = new List<Point2d>();
            foreach (var v in source.Vertices)
                vertices.Add(new Point2d(v.Location.X, v.Location.Y));

            var result = new Polyline(vertices, source.IsClosed)
            {
                Elevation = source.Elevation
            };
            ApplyLayerAndColorToNormal(result, source);
            return result;
        }
    }
}
