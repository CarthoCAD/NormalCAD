using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Media;
using NormalCAD.Core;
using NormalCAD.Core.Entities;

namespace NormalCAD.View.Drawing
{
    public class DrawingService
    {
        private readonly Dictionary<Type, IEntityRenderer> _renderers = new();

        public DrawingService()
        {
            Register<Line>(new LineRenderer());
            Register<Circle>(new CircleRenderer());
            Register<Arc>(new ArcRenderer());
        }

        public void Register<T>(IEntityRenderer renderer) where T : Entity
        {
            _renderers[typeof(T)] = renderer;
        }

        public void DrawDatabase(DrawingContext context, Controller.CadController controller,
                                  Func<Core.Geometry.Point3d, Point> worldToScreen, double zoom)
        {
            var db = controller.Database;
            if (!db.TryGetObject(db.BlockTableId, out var btObj) || btObj is not BlockTable bt)
                return;

            var modelSpaceId = bt[BlockTableRecord.ModelSpace];
            if (modelSpaceId.IsNull || !db.TryGetObject(modelSpaceId, out var btrObj) || btrObj is not BlockTableRecord btr)
                return;

            foreach (var entId in btr.GetEntityIds())
            {
                if (!db.TryGetObject(entId, out var entObj) || entObj is not Entity ent)
                    continue;

                DrawEntity(context, ent, controller, controller.IsSelected(entId),
                           isPreview: false, worldToScreen, zoom);
            }
        }

        public void DrawEntity(DrawingContext context, Entity ent, Controller.CadController controller,
                               bool isSelected, bool isPreview,
                               Func<Core.Geometry.Point3d, Point> worldToScreen, double zoom)
        {
            var baseColor = GetEntityRenderColor(ent, controller.Database, controller.IsLightTheme);
            Color renderColor = isSelected ? Color.Parse("#007ACC") : baseColor;
            if (isPreview)
                renderColor = Color.Parse("#FF9900");

            var brush = new SolidColorBrush(renderColor);
            var pen = new Pen(brush, isSelected ? 3.0 : (isPreview ? 1.0 : 1.5));
            if (isPreview)
                pen.DashStyle = DashStyle.Dash;

            var type = ent.GetType();
            if (_renderers.TryGetValue(type, out var renderer))
                renderer.Render(context, ent, pen, worldToScreen, zoom);
        }

        private static Color GetEntityRenderColor(Entity ent, Database database, bool isLightTheme)
        {
            EntityColor coreColor = ent.Color;
            if (coreColor.IsByLayer)
            {
                if (database.TryGetObject(database.LayerTableId, out var ltObj) && ltObj is LayerTable lt)
                {
                    var layerId = lt[ent.Layer];
                    if (!layerId.IsNull)
                    {
                        var layer = lt.GetRecord(layerId);
                        coreColor = layer.Color;
                    }
                }
            }

            var final = Color.FromArgb(coreColor.A, coreColor.R, coreColor.G, coreColor.B);

            if (isLightTheme && final == Colors.White)
                return Colors.Black;

            return final;
        }
    }
}
