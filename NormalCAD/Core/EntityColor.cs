using System;

namespace NormalCAD.Core
{
    public struct EntityColor : IEquatable<EntityColor>
    {
        public byte R { get; }
        public byte G { get; }
        public byte B { get; }
        public byte A { get; }

        public EntityColor(byte r, byte g, byte b, byte a = 255)
        {
            R = r;
            G = g;
            B = b;
            A = a;
        }

        public static EntityColor White => new EntityColor(255, 255, 255);
        public static EntityColor Black => new EntityColor(0, 0, 0);
        public static EntityColor Red => new EntityColor(255, 0, 0);
        public static EntityColor Green => new EntityColor(0, 255, 0);
        public static EntityColor Blue => new EntityColor(0, 0, 255);
        public static EntityColor Yellow => new EntityColor(255, 255, 0);
        public static EntityColor Cyan => new EntityColor(0, 255, 255);
        public static EntityColor Magenta => new EntityColor(255, 0, 255);
        public static EntityColor ByLayer => new EntityColor(255, 255, 255, 0); // Alpha = 0 representa ByLayer

        public bool IsByLayer => A == 0;

        public bool Equals(EntityColor other) => R == other.R && G == other.G && B == other.B && A == other.A;
        public override bool Equals(object? obj) => obj is EntityColor other && Equals(other);
        public override int GetHashCode() => (R, G, B, A).GetHashCode();

        public static bool operator ==(EntityColor left, EntityColor right) => left.Equals(right);
        public static bool operator !=(EntityColor left, EntityColor right) => !left.Equals(right);
    }
}
