using System;
using Avalonia;
using Avalonia.Media;
using NormalCAD.Core;
using NormalCAD.Core.Geometry;
using NormalCAD.Core.Entities;

namespace NormalCAD.View.Drawing
{
    public class LineRenderer : IEntityRenderer
    {
        public void Render(DrawingContext context, Entity entity, Pen pen,
                           Func<Point3d, Point> worldToScreen, double zoom)
        {
            if (entity is not Line line) return;
            context.DrawLine(pen, worldToScreen(line.StartPoint), worldToScreen(line.EndPoint));
        }
    }
}
