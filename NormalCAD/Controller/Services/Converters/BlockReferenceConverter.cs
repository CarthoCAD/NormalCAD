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
            var blockName = !string.IsNullOrEmpty(source.BlockName)
                ? source.BlockName
                : ResolveBlockName(source.BlockTableRecordId);

            var blockRecord = !string.IsNullOrEmpty(blockName)
                ? ResolveBlockRecord(blockName, cadDoc)
                : new BlockRecord("_Empty");

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

            var result = new BlockReference
            {
                Position = new Point3d(source.InsertPoint.X, source.InsertPoint.Y, source.InsertPoint.Z),
                Rotation = source.Rotation,
                ScaleFactors = new Vector3d(source.XScale, source.YScale, source.ZScale),
                BlockName = blockName
            };

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

        private static BlockRecord ResolveBlockRecord(string blockName, CadDocument cadDoc)
        {
            if (cadDoc.BlockRecords.TryGetValue(blockName, out var existing))
                return existing;

            var newBlock = new BlockRecord(blockName);
            cadDoc.BlockRecords.Add(newBlock);
            return newBlock;
        }
    }
}
