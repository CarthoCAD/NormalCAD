namespace NormalCAD.Core.DatabaseServices
{
    public class BlockTable : SymbolTable<BlockTableRecord>
    {
        public BlockTable(Database database) : base(database)
        {
        }
    }
}
