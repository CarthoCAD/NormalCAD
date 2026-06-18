using System;
using Avalonia;
using Avalonia.Media;
using NormalCAD.Core;
using NormalCAD.Core.Geometry;

namespace NormalCAD.View.Drawing
{
    public interface IEntityRenderer
    {
        void Render(DrawingContext context, Entity entity, Pen pen,
                    Func<Point3d, Point> worldToScreen, double zoom);
    }
}
