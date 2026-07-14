using System;

namespace NormalCAD.Core.DatabaseServices
{
    public struct Handle : IEquatable<Handle>
    {
        public long Value { get; }

        public Handle(long value)
        {
            Value = value;
        }

        public static Handle Null => default;

        public bool IsNull => Value == 0;

        public bool Equals(Handle other) => Value == other.Value;
        public override bool Equals(object? obj) => obj is Handle handle && Equals(handle);
        public override int GetHashCode() => Value.GetHashCode();

        public static bool operator ==(Handle left, Handle right) => left.Equals(right);
        public static bool operator !=(Handle left, Handle right) => !left.Equals(right);

        public override string ToString() => IsNull ? "Null" : Value.ToString("X");
    }
}
