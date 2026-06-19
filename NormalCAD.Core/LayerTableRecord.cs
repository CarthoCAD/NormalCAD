namespace NormalCAD.Core
{
    public class LayerTableRecord : SymbolTableRecord
    {
        public EntityColor Color { get; set; } = EntityColor.White;
        public bool IsVisible { get; set; } = true;

        public LayerTableRecord()
        {
        }

        public LayerTableRecord(string name, EntityColor color)
        {
            Name = name;
            Color = color;
        }
    }
}
