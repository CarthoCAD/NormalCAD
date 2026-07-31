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

        public static void LoadBlockTable(CadDocument cadDoc, Database db, Transaction trans, ConverterService converters)
        {
            if (!db.TryGetObject(db.BlockTableId, out var btObj) || btObj is not BlockTable bt)
                return;

            foreach (var acadBlockRec in cadDoc.BlockRecords)
            {
                if (!bt.Has(acadBlockRec.Name))
                    bt.Add(new BlockTableRecord(acadBlockRec.Name));
            }

            foreach (var acadBlockRec in cadDoc.BlockRecords)
            {
                var btr = bt.GetRecord(bt[acadBlockRec.Name]);
                
                foreach (var acadEntity in acadBlockRec.Entities)
                {
                    var normalEntity = converters.ConvertToNormal(acadEntity);
                    if (normalEntity == null) continue;

                    btr.AppendEntity(normalEntity);
                    trans.AddNewlyCreatedDBObject(normalEntity, true);
                }
            }
        }

        public static void SaveBlockTable(Database db, CadDocument cadDoc, ConverterService converters)
        {
            if (!db.TryGetObject(db.BlockTableId, out var btObj) || btObj is not BlockTable bt)
                return;

            foreach (var record in bt)
            {
                if (!cadDoc.BlockRecords.TryGetValue(record.Name, out _))
                    cadDoc.BlockRecords.Add(new ACadSharp.Tables.BlockRecord(record.Name));
            }

            foreach (var record in bt)
            {
                if (!cadDoc.BlockRecords.TryGetValue(record.Name, out var acadBlockRec))
                    continue;

                foreach (var entId in record.GetEntityIds())
                {
                    if (!db.TryGetObject(entId, out var entObj) || entObj is not NormalCAD.Core.DatabaseServices.Entity normalEnt)
                        continue;

                    var acadEntity = converters.ConvertToAcad(normalEnt, cadDoc);
                    if (acadEntity != null)
                        acadBlockRec.Entities.Add(acadEntity);
                }
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

    }
}
