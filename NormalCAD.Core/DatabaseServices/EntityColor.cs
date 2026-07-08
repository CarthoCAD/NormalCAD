using System;
using System.Collections.Generic;

namespace NormalCAD.Core.DatabaseServices
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
        public static EntityColor ByLayer => new EntityColor(255, 255, 255, 0);
        public static EntityColor ByBlock => new EntityColor(255, 255, 255, 1);

        public bool IsByLayer => A == 0;
        public bool IsByBlock => A == 1 && !IsByLayer;

        public override string ToString()
        {
            if (IsByLayer) return "ByLayer";
            if (IsByBlock) return "ByBlock";
            if (this.Equals(Red)) return "Red";
            if (this.Equals(Green)) return "Green";
            if (this.Equals(Blue)) return "Blue";
            if (this.Equals(Yellow)) return "Yellow";
            if (this.Equals(Cyan)) return "Cyan";
            if (this.Equals(Magenta)) return "Magenta";
            if (this.Equals(White)) return "White";
            if (this.Equals(Black)) return "Black";
            return $"#{R:X2}{G:X2}{B:X2}";
        }

        private static readonly Dictionary<string, EntityColor> NamedColors = new(StringComparer.OrdinalIgnoreCase)
        {
            ["bylayer"] = ByLayer,
            ["byblock"] = ByBlock,
            ["red"] = Red,
            ["green"] = Green,
            ["blue"] = Blue,
            ["yellow"] = Yellow,
            ["cyan"] = Cyan,
            ["magenta"] = Magenta,
            ["white"] = White,
            ["black"] = Black,
        };

        public static bool TryParse(string s, out EntityColor color)
        {
            color = default;
            if (string.IsNullOrWhiteSpace(s)) return false;

            s = s.Trim();

            if (NamedColors.TryGetValue(s, out color))
                return true;

            if (s.StartsWith("#") && s.Length == 7)
            {
                try
                {
                    byte r = Convert.ToByte(s.Substring(1, 2), 16);
                    byte g = Convert.ToByte(s.Substring(3, 2), 16);
                    byte b = Convert.ToByte(s.Substring(5, 2), 16);
                    color = new EntityColor(r, g, b);
                    return true;
                }
                catch { return false; }
            }

            var parts = s.Split(',', ' ', ';');
            if (parts.Length >= 3)
            {
                if (byte.TryParse(parts[0].Trim(), out byte r) &&
                    byte.TryParse(parts[1].Trim(), out byte g) &&
                    byte.TryParse(parts[2].Trim(), out byte b))
                {
                    color = new EntityColor(r, g, b);
                    return true;
                }
            }

            return false;
        }

        public bool Equals(EntityColor other) => R == other.R && G == other.G && B == other.B && A == other.A;
        public override bool Equals(object? obj) => obj is EntityColor other && Equals(other);
        public override int GetHashCode() => (R, G, B, A).GetHashCode();

        public static bool operator ==(EntityColor left, EntityColor right) => left.Equals(right);
        public static bool operator !=(EntityColor left, EntityColor right) => !left.Equals(right);
    }
}
