using System;

namespace NormalCAD.Core.Geometry
{
    public struct Point2d : IEquatable<Point2d>
    {
        public double X { get; }
        public double Y { get; }

        public static Point2d Origin => new Point2d(0, 0);

        public Point2d(double x, double y)
        {
            X = x;
            Y = y;
        }

        public double DistanceTo(Point2d other)
        {
            double dx = X - other.X;
            double dy = Y - other.Y;
            return Math.Sqrt(dx * dx + dy * dy);
        }

        public Point3d ToPoint3d(double z = 0) => new Point3d(X, Y, z);

        public static Point2d FromPoint3d(Point3d pt) => new Point2d(pt.X, pt.Y);

        public bool Equals(Point2d other) => X == other.X && Y == other.Y;
        public override bool Equals(object? obj) => obj is Point2d other && Equals(other);
        public override int GetHashCode() => (X, Y).GetHashCode();

        public static bool operator ==(Point2d left, Point2d right) => left.Equals(right);
        public static bool operator !=(Point2d left, Point2d right) => !left.Equals(right);
    }
}
