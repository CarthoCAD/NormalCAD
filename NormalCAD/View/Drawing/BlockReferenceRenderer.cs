using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Media;
using NormalCAD.Core.DatabaseServices;
using NormalCAD.Core.Geometry;

namespace NormalCAD.View.Drawing
{
    public class BlockReferenceRenderer : IEntityRenderer
    {
        private const int CircleApproximationSegments = 36;

        public bool IsCompound => true;

        public IEnumerable<Entity> ExpandForRender(Entity entity)
        {
            if (entity is not BlockReference br) yield break;
            
            var db = br.Database;
            if (db == null) yield break;

            if (br.BlockTableRecord.IsNull) yield break;
            if (!db.TryGetObject(br.BlockTableRecord, out var obj)
                || obj is not BlockTableRecord btr) yield break;

            bool nonUniform = Math.Abs(br.ScaleFactors.X - br.ScaleFactors.Y) > 1e-9;

            foreach (var entId in btr.GetEntityIds())
            {
                if (!db.TryGetObject(entId, out var entObj)
                    || entObj is not Entity subEnt) continue;

                Entity renderEnt = nonUniform ? TryAdjustToPoly(subEnt) : subEnt;
                renderEnt = renderEnt.GetTransformedCopy(br.BlockTransform);

                if (renderEnt.LayerId == db.LayerZero)
                {
                    renderEnt.LayerId = br.LayerId;
                }

                yield return renderEnt;
            }
        }

        public void Render(DrawingContext context, Entity entity, Pen pen,
                           Func<Point3d, Point> worldToScreen, double zoom)
        {
        }

        private static Entity TryAdjustToPoly(Entity entity)
        {
            return entity switch
            {
                Circle circle => CircleToPolyline(circle),
                Arc arc => ArcToPolyline(arc),
                _ => entity,
            };

        }

        private static Polyline CircleToPolyline(Circle circle)
        {
            var poly = new Polyline(CircleApproximationSegments) { Closed = true };
            CopyVisualProperties(poly, circle);
            for (int i = 0; i < CircleApproximationSegments; i++)
            {
                double angle = 2.0 * Math.PI * i / CircleApproximationSegments;
                double x = circle.Center.X + circle.Radius * Math.Cos(angle);
                double y = circle.Center.Y + circle.Radius * Math.Sin(angle);
                poly.AddVertexAt(i, new Point2d(x, y), 0, 0, 0);
            }
            return poly;
        }

        private static Polyline ArcToPolyline(Arc arc)
        {
            int segments = Math.Max(2, (int)(CircleApproximationSegments * arc.TotalAngle / (2.0 * Math.PI)));
            var poly = new Polyline(segments) { Closed = false };
            CopyVisualProperties(poly, arc);
            for (int i = 0; i <= segments; i++)
            {
                double angle = arc.StartAngle + arc.TotalAngle * i / segments;
                double x = arc.Center.X + arc.Radius * Math.Cos(angle);
                double y = arc.Center.Y + arc.Radius * Math.Sin(angle);
                poly.AddVertexAt(i, new Point2d(x, y), 0, 0, 0);
            }
            return poly;
        }

        private static void CopyVisualProperties(Entity target, Entity source)
        {
            target.LayerId = source.LayerId;
            target.Color = source.Color;
            target.Linetype = source.Linetype;
            target.LineWeight = source.LineWeight;
            target.LinetypeScale = source.LinetypeScale;
            target.Transparency = source.Transparency;
            target.Visible = source.Visible;
        }
    }
}
