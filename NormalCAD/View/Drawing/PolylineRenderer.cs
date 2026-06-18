using System;
using Avalonia;
using Avalonia.Media;
using NormalCAD.Core;
using NormalCAD.Core.Geometry;
using NormalCAD.Core.Entities;

namespace NormalCAD.View.Drawing
{
    public class PolylineRenderer : IEntityRenderer
    {
        public void Render(DrawingContext context, Entity entity, Pen pen,
                           Func<Point3d, Point> worldToScreen, double zoom)
        {
            if (entity is not LwPolyline poly || poly.Vertices.Count < 2) return;

            for (int i = 0; i < poly.Vertices.Count - 1; i++)
            {
                context.DrawLine(pen,
                    worldToScreen(poly.Vertices[i]),
                    worldToScreen(poly.Vertices[i + 1]));
            }

            if (poly.IsClosed)
            {
                context.DrawLine(pen,
                    worldToScreen(poly.Vertices[poly.Vertices.Count - 1]),
                    worldToScreen(poly.Vertices[0]));
            }
        }
    }
}
