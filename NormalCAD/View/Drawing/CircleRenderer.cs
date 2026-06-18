using System;
using Avalonia;
using Avalonia.Media;
using NormalCAD.Core;
using NormalCAD.Core.Geometry;
using NormalCAD.Core.Entities;

namespace NormalCAD.View.Drawing
{
    public class CircleRenderer : IEntityRenderer
    {
        public void Render(DrawingContext context, Entity entity, Pen pen,
                           Func<Point3d, Point> worldToScreen, double zoom)
        {
            if (entity is not Circle circle) return;
            var center = worldToScreen(circle.Center);
            var radius = circle.Radius * zoom;
            context.DrawEllipse(null, pen, center, radius, radius);
        }
    }
}
