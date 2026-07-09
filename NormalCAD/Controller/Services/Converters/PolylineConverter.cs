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
            var result = new ACadSharp.Entities.LwPolyline
            {
                IsClosed = source.Closed,
                Elevation = source.Elevation
            };

            for (int i = 0; i < source.NumberOfVertices; i++)
            {
                var pt = source.GetPoint2dAt(i);
                source.GetWidthsAt(i, out double startWidth, out double endWidth);
                result.Vertices.Add(new ACadSharp.Entities.LwPolyline.Vertex(new XY(pt.X, pt.Y))
                {
                    StartWidth = startWidth,
                    EndWidth = endWidth,
                    Bulge = source.GetBulgeAt(i)
                });
            }

            ApplyEntityPropertiesToAcad(result, source, cadDoc);
            result.Normal = new XYZ(source.Normal.X, source.Normal.Y, source.Normal.Z);
            result.Thickness = source.Thickness;
            result.ConstantWidth = source.ConstantWidth;
            return result;
        }

        public override Polyline ConvertToNormal(ACadSharp.Entities.LwPolyline source)
        {
            var result = new Polyline
            {
                Closed = source.IsClosed,
                Elevation = source.Elevation,
                Normal = new Vector3d(source.Normal.X, source.Normal.Y, source.Normal.Z),
                Thickness = source.Thickness,
                ConstantWidth = source.ConstantWidth
            };

            int i = 0;
            foreach (var v in source.Vertices)
            {
                result.AddVertexAt(i++, new Point2d(v.Location.X, v.Location.Y),
                    v.Bulge, v.StartWidth, v.EndWidth);
            }

            ApplyEntityPropertiesToNormal(result, source);
            return result;
        }
    }
}
