using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Media;
using NormalCAD.Core.DatabaseServices;
using NormalCAD.Core.Geometry;

namespace NormalCAD.View.Drawing
{
    public interface IEntityRenderer
    {
        bool IsCompound => false;
        IEnumerable<Entity> ExpandForRender(Entity entity) => [];

        void Render(DrawingContext context, Entity entity, Pen pen,
                    Func<Point3d, Point> worldToScreen, double zoom);
    }
}
