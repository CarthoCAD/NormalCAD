using System;
using NormalCAD.Core.DatabaseServices;

namespace NormalCAD.Core.Geometry
{
    public struct Point3d
    {
        public double X { get; }
        public double Y { get; }
        public double Z { get; }

        public Point3d(double x, double y, double z = 0.0)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public static Point3d Origin => new Point3d(0, 0, 0);

        public double DistanceTo(Point3d other)
        {
            double dx = X - other.X;
            double dy = Y - other.Y;
            double dz = Z - other.Z;
            return Math.Sqrt(dx * dx + dy * dy + dz * dz);
        }

        public static Point3d operator +(Point3d p, Vector3d v) => new Point3d(p.X + v.X, p.Y + v.Y, p.Z + v.Z);
        public static Point3d operator -(Point3d p, Vector3d v) => new Point3d(p.X - v.X, p.Y - v.Y, p.Z - v.Z);
        public static Vector3d operator -(Point3d p1, Point3d p2) => new Vector3d(p1.X - p2.X, p1.Y - p2.Y, p1.Z - p2.Z);

        private const int Precision = 4;
        private static readonly string _format = $"F{Precision}";

        public override string ToString() =>
            $"{Culture.ToString(X, _format)},{Culture.ToString(Y, _format)},{Culture.ToString(Z, _format)}";

        public string ToString2D() =>
            $"{Culture.ToString(X, _format)},{Culture.ToString(Y, _format)}";

        public static Point3d Parse(string s)
        {
            if (TryParse(s, out var result))
                return result;
            throw new FormatException($"Invalid Point3d format: '{s}'. Expected X,Y or X,Y,Z.");
        }

        public static bool TryParse(string s, out Point3d result)
        {
            result = Origin;
            var parts = s.Split(',');

            if (parts.Length < 2 || parts.Length > 3) return false;
            if (!Culture.TryParse(parts[0], out double x)) return false;
            if (!Culture.TryParse(parts[1], out double y)) return false;

            double z = 0;
            if (parts.Length == 3 && !Culture.TryParse(parts[2], out z)) return false;

            result = new Point3d(x, y, z);
            return true;
        }
    }
}
