using NormalCAD.Core.DatabaseServices;
using ACadSharp;
using CadDocument = ACadSharp.CadDocument;

namespace NormalCAD.Controller.Services.Converters
{
    public static class TableParsers
    {
        public static void LoadLayers(CadDocument cadDoc, Database db, ConverterService converters)
        {
            if (!db.TryGetObject(db.LayerTableId, out var ltObj) || ltObj is not LayerTable lt)
                return;

            foreach (var acadLayer in cadDoc.Layers)
            {
                var layerRec = converters.ConvertLayerToNormal(acadLayer);
                if (layerRec == null) continue;

                if (lt.Has(layerRec.Name))
                {
                    var existingId = lt[layerRec.Name];
                    var existingRec = lt.GetRecord(existingId);
                    existingRec.Color = layerRec.Color;
                }
                else
                {
                    lt.Add(layerRec);
                }
            }
        }

        public static void LoadViewports(CadDocument cadDoc, Database db, ConverterService converters)
        {
            if (!db.TryGetObject(db.ViewportTableId, out var vtObj) || vtObj is not ViewportTable vt)
                return;

            foreach (var acadVPort in cadDoc.VPorts)
            {
                var vpr = converters.ConvertVPortToNormal(acadVPort);
                if (vpr == null) continue;

                if (vt.Has(vpr.Name))
                {
                    var existingId = vt[vpr.Name];
                    var existingRec = vt.GetRecord(existingId);
                    existingRec.Center = vpr.Center;
                    existingRec.ViewHeight = vpr.ViewHeight;
                    existingRec.Direction = vpr.Direction;
                    existingRec.Target = vpr.Target;
                }
                else
                {
                    vt.Add(vpr);
                }
            }
        }

        public static void LoadEntities(CadDocument cadDoc, Database db, Transaction trans, ConverterService converters)
        {
            if (!db.TryGetObject(db.BlockTableId, out var btObj) || btObj is not BlockTable bt)
                return;

            var modelSpaceId = bt[BlockTableRecord.ModelSpace];
            if (!db.TryGetObject(modelSpaceId, out var btrObj) || btrObj is not BlockTableRecord btr)
                return;

            foreach (var acadEntity in cadDoc.Entities)
            {
                var normalEntity = converters.ConvertToNormal(acadEntity);
                if (normalEntity == null) continue;

                btr.AppendEntity(normalEntity);
                trans.AddNewlyCreatedDBObject(normalEntity, true);
            }
        }

        public static void SaveLayers(Database db, CadDocument cadDoc, ConverterService converters)
        {
            if (!db.TryGetObject(db.LayerTableId, out var ltObj) || ltObj is not LayerTable lt)
                return;

            foreach (var layerRec in lt)
            {
                if (layerRec.Name == "0") continue;

                var acadLayer = converters.ConvertLayerToAcad(layerRec);
                if (acadLayer != null)
                    cadDoc.Layers.Add(acadLayer);
            }
        }

        public static void SaveViewports(Database db, CadDocument cadDoc, ConverterService converters)
        {
            if (!db.TryGetObject(db.ViewportTableId, out var vtObj) || vtObj is not ViewportTable vt)
                return;

            foreach (var vpr in vt)
            {
                var acadVPort = converters.ConvertVPortToAcad(vpr);
                if (acadVPort == null) continue;

                if (cadDoc.VPorts.TryGetValue(acadVPort.Name, out var existing))
                {
                    converters.ApplyVPortToAcad(vpr, existing);
                }
                else
                {
                    cadDoc.VPorts.Add(acadVPort);
                }
            }
        }

        public static void SaveEntities(Database db, CadDocument cadDoc, ConverterService converters)
        {
            if (!db.TryGetObject(db.BlockTableId, out var btObj) || btObj is not BlockTable bt)
                return;

            var modelSpaceId = bt[BlockTableRecord.ModelSpace];
            if (modelSpaceId.IsNull || !db.TryGetObject(modelSpaceId, out var btrObj) || btrObj is not BlockTableRecord btr)
                return;

            foreach (var entId in btr.GetEntityIds())
            {
                if (!db.TryGetObject(entId, out var entObj) || entObj is not NormalCAD.Core.DatabaseServices.Entity normalEnt)
                    continue;

                var acadEntity = converters.ConvertToAcad(normalEnt, cadDoc);
                if (acadEntity != null)
                    cadDoc.Entities.Add(acadEntity);
            }
        }
    }
}
