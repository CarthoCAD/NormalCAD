using System;

namespace NormalCAD.Core.Geometry
{
    public struct Extents3d
    {
        public Point3d MinPoint { get; set; }
        public Point3d MaxPoint { get; set; }

        public Extents3d(Point3d min, Point3d max)
        {
            MinPoint = min;
            MaxPoint = max;
        }

        public static Extents3d FromPoints(params Point3d[] points)
        {
            if (points == null || points.Length == 0)
                return new Extents3d(Point3d.Origin, Point3d.Origin);

            double minX = points[0].X, minY = points[0].Y, minZ = points[0].Z;
            double maxX = points[0].X, maxY = points[0].Y, maxZ = points[0].Z;

            foreach (var pt in points)
            {
                minX = Math.Min(minX, pt.X);
                minY = Math.Min(minY, pt.Y);
                minZ = Math.Min(minZ, pt.Z);
                maxX = Math.Max(maxX, pt.X);
                maxY = Math.Max(maxY, pt.Y);
                maxZ = Math.Max(maxZ, pt.Z);
            }

            return new Extents3d(
                new Point3d(minX, minY, minZ),
                new Point3d(maxX, maxY, maxZ));
        }

        public bool Contains(Point3d pt)
        {
            return pt.X >= MinPoint.X && pt.X <= MaxPoint.X &&
                   pt.Y >= MinPoint.Y && pt.Y <= MaxPoint.Y;
        }

        public bool Intersects(Extents3d other)
        {
            return MinPoint.X <= other.MaxPoint.X && MaxPoint.X >= other.MinPoint.X &&
                   MinPoint.Y <= other.MaxPoint.Y && MaxPoint.Y >= other.MinPoint.Y;
        }
    }
}
