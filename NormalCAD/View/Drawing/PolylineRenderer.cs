using System;
using Avalonia;
using Avalonia.Media;
using NormalCAD.Core.DatabaseServices;
using NormalCAD.Core.Geometry;

namespace NormalCAD.View.Drawing
{
    public class PolylineRenderer : IEntityRenderer
    {
        public void Render(DrawingContext context, Entity entity, Pen pen,
                           Func<Point3d, Point> worldToScreen, double zoom)
        {
            if (entity is not Polyline poly || poly.NumberOfVertices < 2) return;

            for (int i = 0; i < poly.NumberOfVertices - 1; i++)
            {
                context.DrawLine(pen,
                    worldToScreen(poly.GetPoint3dAt(i)),
                    worldToScreen(poly.GetPoint3dAt(i + 1)));
            }

            if (poly.Closed)
            {
                context.DrawLine(pen,
                    worldToScreen(poly.GetPoint3dAt(poly.NumberOfVertices - 1)),
                    worldToScreen(poly.GetPoint3dAt(0)));
            }
        }
    }
}