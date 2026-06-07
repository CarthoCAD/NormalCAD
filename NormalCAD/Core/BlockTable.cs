namespace NormalCAD.Core
{
    public class BlockTable : SymbolTable<BlockTableRecord>
    {
        public BlockTable(Database database) : base(database)
        {
        }
    }
}
