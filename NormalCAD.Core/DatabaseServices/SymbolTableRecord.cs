namespace NormalCAD.Core.DatabaseServices
{
    public abstract class SymbolTableRecord : DBObject
    {
        public string Name { get; set; } = string.Empty;
    }
}
