namespace NormalCAD.Core.DatabaseServices
{
    public class LayerTable : SymbolTable<LayerTableRecord>
    {
        public LayerTable(Database database) : base(database)
        {
        }
    }
}
