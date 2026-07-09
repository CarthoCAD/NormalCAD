using System;

namespace NormalCAD.Core.DatabaseServices
{
    public struct Transparency : IEquatable<Transparency>
    {
        public byte Alpha { get; }

        public Transparency(byte alpha)
        {
            Alpha = alpha;
        }

        public static Transparency ByLayer => new Transparency(0);
        public static Transparency ByBlock => new Transparency(1);

        public bool IsByLayer => Alpha == 0;
        public bool IsByBlock => Alpha == 1;

        public static Transparency FromAlpha(byte alpha) => new Transparency(alpha);

        public override string ToString() => IsByLayer ? "ByLayer" : IsByBlock ? "ByBlock" : $"{90 - (int)(Alpha * 90.0 / 255)}";

        public static bool TryParse(string s, out Transparency transparency)
        {
            transparency = default;
            if (string.IsNullOrWhiteSpace(s)) return false;
            s = s.Trim();

            if (s.Equals("ByLayer", StringComparison.OrdinalIgnoreCase))
            {
                transparency = ByLayer;
                return true;
            }

            if (s.Equals("ByBlock", StringComparison.OrdinalIgnoreCase))
            {
                transparency = ByBlock;
                return true;
            }

            if (int.TryParse(s, out int percent) && percent >= 0 && percent <= 90)
            {
                byte alpha = (byte)((90 - percent) * 255 / 90);
                transparency = new Transparency(alpha);
                return true;
            }

            return false;
        }

        public bool Equals(Transparency other) => Alpha == other.Alpha;
        public override bool Equals(object? obj) => obj is Transparency other && Equals(other);
        public override int GetHashCode() => Alpha.GetHashCode();

        public static bool operator ==(Transparency left, Transparency right) => left.Equals(right);
        public static bool operator !=(Transparency left, Transparency right) => !left.Equals(right);
    }
}
