using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Media;
using NormalCAD.Core.DatabaseServices;
using CoreApp = NormalCAD.Core.ApplicationServices.Application;

namespace NormalCAD.View.Drawing
{
    public class DrawingService
    {
        private readonly Dictionary<Type, IEntityRenderer> _renderers = new();
        private readonly Dictionary<string, Color> _layerColorCache = new(StringComparer.OrdinalIgnoreCase);
        private Database? _subscribedDb;

        public DrawingService()
        {
            Register<Line>(new LineRenderer());
            Register<Circle>(new CircleRenderer());
            Register<Arc>(new ArcRenderer());
            Register<Polyline>(new PolylineRenderer());
        }

        public void Register<T>(IEntityRenderer renderer) where T : Entity
        {
            _renderers[typeof(T)] = renderer;
        }

        public void DrawDatabase(DrawingContext context, Controller.CadController controller,
                                  Func<Core.Geometry.Point3d, Point> worldToScreen, double zoom)
        {
            EnsureSubscribed();

            var db = CoreApp.DocumentManager.MdiActiveDocument?.Database;
            if (db == null) return;

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
            EnsureSubscribed();

            var db = CoreApp.DocumentManager.MdiActiveDocument?.Database;
            var baseColor = db != null
                ? ResolveEntityColor(ent, db, controller.IsLightTheme)
                : Colors.White;
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

        private void EnsureSubscribed()
        {
            var db = CoreApp.DocumentManager.MdiActiveDocument?.Database;
            if (db == null) return;
            if (_subscribedDb == db) return;
            if (_subscribedDb != null)
                _subscribedDb.ObjectModified -= OnDatabaseObjectModified;
            _subscribedDb = db;
            db.ObjectModified += OnDatabaseObjectModified;
        }

        private void OnDatabaseObjectModified(object? sender, ObjectEventArgs e)
        {
            if (e.DBObject is LayerTableRecord)
                _layerColorCache.Clear();
        }

        private Color ResolveEntityColor(Entity ent, Database database, bool isLightTheme)
        {
            if (!ent.Color.IsByLayer)
            {
                var final = Color.FromArgb(ent.Color.A, ent.Color.R, ent.Color.G, ent.Color.B);
                if (isLightTheme && final == Colors.White)
                    return Colors.Black;
                return final;
            }

            // ByLayer: check cache first
            if (!_layerColorCache.TryGetValue(ent.Layer, out var cachedColor))
            {
                EntityColor coreColor = EntityColor.White;
                if (database.TryGetObject(database.LayerTableId, out var ltObj) && ltObj is LayerTable lt)
                {
                    var layerId = lt[ent.Layer];
                    if (!layerId.IsNull)
                        coreColor = lt.GetRecord(layerId).Color;
                }
                cachedColor = Color.FromArgb(coreColor.A, coreColor.R, coreColor.G, coreColor.B);
                _layerColorCache[ent.Layer] = cachedColor;
            }

            if (isLightTheme && cachedColor == Colors.White)
                return Colors.Black;

            return cachedColor;
        }
    }
}
