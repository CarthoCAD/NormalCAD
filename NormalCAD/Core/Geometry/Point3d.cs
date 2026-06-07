using System;

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

        public override string ToString() => $"({X:F4}, {Y:F4}, {Z:F4})";
    }
}
