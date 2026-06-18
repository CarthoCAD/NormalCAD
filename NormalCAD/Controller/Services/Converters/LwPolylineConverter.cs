using System.Collections.Generic;
using NormalCAD.Core.Entities;
using NormalCAD.Core.Geometry;

namespace NormalCAD.Controller.Services.Converters
{
    public class LwPolylineConverter : IConverter
    {
        public bool CanConvertToAcad => false;
        public bool CanConvertToNormal => true;

        public IEnumerable<Line> ConvertToNormal(ACadSharp.Entities.LwPolyline source)
        {
            var lines = new List<Line>();

            if (source.Vertices.Count < 2)
                return lines;

            var layerName = source.Layer?.Name ?? "0";
            var entityColor = ColorConverter.ToEntityColor(source.Color);

            for (int i = 0; i < source.Vertices.Count - 1; i++)
            {
                var v1 = source.Vertices[i];
                var v2 = source.Vertices[i + 1];
                lines.Add(new Line(
                    new Point3d(v1.Location.X, v1.Location.Y, 0.0),
                    new Point3d(v2.Location.X, v2.Location.Y, 0.0)
                )
                {
                    Layer = layerName,
                    Color = entityColor
                });
            }

            if (source.IsClosed)
            {
                var vLast = source.Vertices[source.Vertices.Count - 1];
                var vFirst = source.Vertices[0];
                lines.Add(new Line(
                    new Point3d(vLast.Location.X, vLast.Location.Y, 0.0),
                    new Point3d(vFirst.Location.X, vFirst.Location.Y, 0.0)
                )
                {
                    Layer = layerName,
                    Color = entityColor
                });
            }

            return lines;
        }
    }
}
