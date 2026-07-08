using System;

namespace NormalCAD.Core.Geometry
{
    public struct Vector3d
    {
        public double X { get; }
        public double Y { get; }
        public double Z { get; }

        public Vector3d(double x, double y, double z = 0.0)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public double Length => Math.Sqrt(X * X + Y * Y + Z * Z);

        public static Vector3d XAxis => new Vector3d(1, 0, 0);
        public static Vector3d YAxis => new Vector3d(0, 1, 0);
        public static Vector3d ZAxis => new Vector3d(0, 0, 1);

        public Vector3d Normalize()
        {
            double len = Length;
            if (len == 0) return this;
            return new Vector3d(X / len, Y / len, Z / len);
        }

        public static Vector3d operator +(Vector3d v1, Vector3d v2) => new Vector3d(v1.X + v2.X, v1.Y + v2.Y, v1.Z + v2.Z);
        public static Vector3d operator -(Vector3d v1, Vector3d v2) => new Vector3d(v1.X - v2.X, v1.Y - v2.Y, v1.Z - v2.Z);
        public static Vector3d operator *(Vector3d v, double scale) => new Vector3d(v.X * scale, v.Y * scale, v.Z * scale);
        public static Vector3d operator /(Vector3d v, double divisor) => new Vector3d(v.X / divisor, v.Y / divisor, v.Z / divisor);

        public double DotProduct(Vector3d other) => X * other.X + Y * other.Y + Z * other.Z;

        public override string ToString() => $"[{X:F4}, {Y:F4}, {Z:F4}]";
    }
}
