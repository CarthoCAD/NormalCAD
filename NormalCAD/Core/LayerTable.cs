namespace NormalCAD.Core
{
    public class LayerTable : SymbolTable<LayerTableRecord>
    {
        public LayerTable(Database database) : base(database)
        {
        }
    }
}
