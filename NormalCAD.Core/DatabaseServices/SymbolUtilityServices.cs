namespace NormalCAD.Core.DatabaseServices
{
    public static class SymbolUtilityServices
    {
        public static string BlockModelSpaceName => BlockTableRecord.ModelSpace;

        public static string BlockPaperSpaceName => BlockTableRecord.PaperSpace;

        public static ObjectId GetBlockModelSpaceId(Database db)
        {
            if (!db.TryGetObject(db.BlockTableId, out var btObj) || btObj is not BlockTable bt)
                return ObjectId.Null;
            return bt[BlockTableRecord.ModelSpace];
        }

        public static ObjectId GetBlockPaperSpaceId(Database db)
        {
            if (!db.TryGetObject(db.BlockTableId, out var btObj) || btObj is not BlockTable bt)
                return ObjectId.Null;
            return bt[BlockTableRecord.PaperSpace];
        }
    }
}
