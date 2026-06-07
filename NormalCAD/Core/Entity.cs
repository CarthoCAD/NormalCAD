using System;

namespace NormalCAD.Core
{
    public abstract class Entity : DBObject
    {
        public string Layer { get; set; } = "0";
        public EntityColor Color { get; set; } = EntityColor.ByLayer;

        public abstract Entity Clone();
    }
}
