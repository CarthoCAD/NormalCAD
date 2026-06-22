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

        public bool IsByLayer => Alpha == 0;

        public static Transparency FromAlpha(byte alpha) => new Transparency(alpha);

        public bool Equals(Transparency other) => Alpha == other.Alpha;
        public override bool Equals(object? obj) => obj is Transparency other && Equals(other);
        public override int GetHashCode() => Alpha.GetHashCode();

        public static bool operator ==(Transparency left, Transparency right) => left.Equals(right);
        public static bool operator !=(Transparency left, Transparency right) => !left.Equals(right);
    }
}
