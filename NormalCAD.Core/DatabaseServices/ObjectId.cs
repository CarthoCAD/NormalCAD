using System;

namespace NormalCAD.Core.DatabaseServices
{
    public struct ObjectId : IEquatable<ObjectId>
    {
        public long Value { get; }
        public Database Database { get; }

        public ObjectId(long value, Database database)
        {
            Value = value;
            Database = database;
        }

        public static ObjectId Null => new ObjectId(0, null!);

        public bool IsNull => Value == 0;

        public bool Equals(ObjectId other) => Value == other.Value && Database == other.Database;
        public override bool Equals(object? obj) => obj is ObjectId id && Equals(id);
        public override int GetHashCode() => Value.GetHashCode() ^ (Database?.GetHashCode() ?? 0);

        public static bool operator ==(ObjectId left, ObjectId right) => left.Equals(right);
        public static bool operator !=(ObjectId left, ObjectId right) => !left.Equals(right);

        public override string ToString() => IsNull ? "Null" : Value.ToString();
    }
}
