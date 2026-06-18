namespace NormalCAD.Core
{
    public class ViewportTable : SymbolTable<ViewportTableRecord>
    {
        public const string ActiveViewport = "*Active";

        public ViewportTable(Database database) : base(database)
        {
        }
    }
}
