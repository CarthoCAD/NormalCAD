namespace NormalCAD.Core.DatabaseServices
{
    public class ViewportTable : SymbolTable<ViewportTableRecord>
    {
        public const string ActiveViewport = "*Active";

        public ViewportTable(Database database) : base(database)
        {
        }
    }
}
