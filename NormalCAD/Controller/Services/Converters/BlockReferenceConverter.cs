using System;
using ACadSharp;
using ACadSharp.Entities;
using ACadSharp.Tables;
using NormalCAD.Core.DatabaseServices;
using NormalCAD.Core.Geometry;
using CSMath;

namespace NormalCAD.Controller.Services.Converters
{
    public class BlockReferenceConverter : EntityConverter<BlockReference, Insert>
    {
        public override Insert ConvertToAcad(BlockReference source, CadDocument cadDoc)
        {
            var blockName = ResolveBlockName(source.BlockTableRecord);
            if (string.IsNullOrEmpty(blockName))
                throw new InvalidOperationException("BlockReference has no associated BlockTableRecord.");

            if (!cadDoc.BlockRecords.TryGetValue(blockName, out var blockRecord))
                throw new InvalidOperationException($"BlockRecord '{blockName}' not found in target document.");

            var result = new Insert(blockRecord)
            {
                InsertPoint = new XYZ(source.Position.X, source.Position.Y, source.Position.Z),
                Rotation = source.Rotation,
                XScale = source.ScaleFactors.X,
                YScale = source.ScaleFactors.Y,
                ZScale = source.ScaleFactors.Z
            };

            ApplyEntityPropertiesToAcad(result, source, cadDoc);
            return result;
        }

        public override BlockReference ConvertToNormal(Insert source)
        {
            var blockName = source.Block?.Name ?? string.Empty;
            var btrId = ObjectId.Null;

            var db = Core.ApplicationServices.Application.DocumentManager.MdiActiveDocument?.Database;
            if (db != null && !string.IsNullOrEmpty(blockName)
                && db.TryGetObject(db.BlockTableId, out var btObj) && btObj is BlockTable bt)
            {
                btrId = bt[blockName];
            }

            var result = new BlockReference(
                new Point3d(source.InsertPoint.X, source.InsertPoint.Y, source.InsertPoint.Z),
                btrId);
            result.Rotation = source.Rotation;
            result.ScaleFactors = new Vector3d(source.XScale, source.YScale, source.ZScale);
            ApplyEntityPropertiesToNormal(result, source);
            return result;
        }

        private static string ResolveBlockName(ObjectId blockId)
        {
            if (blockId.IsNull) return string.Empty;
            var db = blockId.Database;
            if (db == null) return string.Empty;
            if (!db.TryGetObject(blockId, out var obj) || obj is not BlockTableRecord btr)
                return string.Empty;
            return btr.Name;
        }
    }
}
