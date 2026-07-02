using System;

namespace NormalCAD.Core.Geometry
{
    public class Matrix3d
    {
        private readonly double[,] _m = new double[4, 4];

        public Matrix3d()
        {
            // Identity
            _m[0, 0] = 1; _m[1, 1] = 1; _m[2, 2] = 1; _m[3, 3] = 1;
        }

        public double this[int row, int col]
        {
            get => _m[row, col];
            set => _m[row, col] = value;
        }

        public static Matrix3d Identity => new Matrix3d();

        public static Matrix3d Translation(Vector3d offset)
        {
            var m = Identity;
            m._m[0, 3] = offset.X;
            m._m[1, 3] = offset.Y;
            m._m[2, 3] = offset.Z;
            return m;
        }

        public static Matrix3d Rotation(double angleRadians, Vector3d axis, Point3d center)
        {
            var cos = Math.Cos(angleRadians);
            var sin = Math.Sin(angleRadians);
            var m = Identity;

            // Rotation about Z (2D)
            m._m[0, 0] = cos;
            m._m[0, 1] = -sin;
            m._m[0, 3] = center.X * (1 - cos) + center.Y * sin;
            m._m[1, 0] = sin;
            m._m[1, 1] = cos;
            m._m[1, 3] = center.Y * (1 - cos) - center.X * sin;

            return m;
        }

        public static Matrix3d Scaling(double scaleFactor, Point3d center)
        {
            var m = Identity;
            m._m[0, 0] = scaleFactor;
            m._m[1, 1] = scaleFactor;
            m._m[2, 2] = scaleFactor;
            m._m[0, 3] = center.X * (1 - scaleFactor);
            m._m[1, 3] = center.Y * (1 - scaleFactor);
            m._m[2, 3] = center.Z * (1 - scaleFactor);
            return m;
        }

        public Point3d TransformPoint(Point3d pt)
        {
            var w = _m[3, 0] * pt.X + _m[3, 1] * pt.Y + _m[3, 2] * pt.Z + _m[3, 3];
            return new Point3d(
                (_m[0, 0] * pt.X + _m[0, 1] * pt.Y + _m[0, 2] * pt.Z + _m[0, 3]) / w,
                (_m[1, 0] * pt.X + _m[1, 1] * pt.Y + _m[1, 2] * pt.Z + _m[1, 3]) / w,
                (_m[2, 0] * pt.X + _m[2, 1] * pt.Y + _m[2, 2] * pt.Z + _m[2, 3]) / w
            );
        }

        public Vector3d TransformVector(Vector3d v)
        {
            return new Vector3d(
                _m[0, 0] * v.X + _m[0, 1] * v.Y + _m[0, 2] * v.Z,
                _m[1, 0] * v.X + _m[1, 1] * v.Y + _m[1, 2] * v.Z,
                _m[2, 0] * v.X + _m[2, 1] * v.Y + _m[2, 2] * v.Z
            );
        }
    }
}
