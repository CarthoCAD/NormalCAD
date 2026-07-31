using System;
using System.Collections.Generic;
using NormalCAD.Core.Geometry;

namespace NormalCAD.Core.DatabaseServices
{
    public class BlockReference : Entity
    {
        private Point3d _position;
        private double _rotation;
        private Vector3d _scaleFactors;

        public double Rotation
        {
            get => _rotation;
            set { _rotation = value; UpdateBlockTransform(); }
        }

        
        public Point3d Position
        {
            get => _position;
            set { _position = value; UpdateBlockTransform(); }
        }

        
        public Vector3d ScaleFactors
        {
            get => _scaleFactors;
            set { _scaleFactors = value; UpdateBlockTransform(); }
        }

        
        public ObjectId BlockTableRecord { get; set; }

        public string Name
        {
            get
            {
                if (!BlockTableRecord.IsNull && Database != null
                    && Database.TryGetObject(BlockTableRecord, out var obj) && obj is BlockTableRecord btr)
                    return btr.Name;
                return string.Empty;
            }
        }

        public Vector3d Normal { get; set; } = Vector3d.ZAxis;

        private Matrix3d _blockTransform = Matrix3d.Identity;
        public override Matrix3d BlockTransform => _blockTransform;

        public BlockReference(Point3d position, ObjectId blockTableRecord)
        {
            _position = position;
            _rotation = 0.0;
            _scaleFactors = new Vector3d(1, 1, 1);
            BlockTableRecord = blockTableRecord;
        }

        private void UpdateBlockTransform()
        {
            var cos = Math.Cos(_rotation);
            var sin = Math.Sin(_rotation);

            _blockTransform = new Matrix3d();
            _blockTransform[0, 0] = cos * _scaleFactors.X;
            _blockTransform[0, 1] = -sin * _scaleFactors.Y;
            _blockTransform[0, 3] = _position.X;
            _blockTransform[1, 0] = sin * _scaleFactors.X;
            _blockTransform[1, 1] = cos * _scaleFactors.Y;
            _blockTransform[1, 3] = _position.Y;
            _blockTransform[2, 2] = _scaleFactors.Z;
            _blockTransform[2, 3] = _position.Z;
            _blockTransform[3, 3] = 1;
        }

        private void SetScaleFactor(double x, double y, double z)
        {
            _scaleFactors = new Vector3d(x, y, z);
            UpdateBlockTransform();
        }

        public override Entity Clone()
        {
            var clone = new BlockReference(_position, BlockTableRecord)
            {
                Rotation = _rotation,
                ScaleFactors = _scaleFactors
            };
            CopyEntityPropertiesTo(clone);
            return clone;
        }

        public override Extents3d GeometricExtents
        {
            get
            {
                if (BlockTableRecord.IsNull || Database == null)
                    return new Extents3d(_position, _position);

                if (Database.TryGetObject(BlockTableRecord, out var obj) && obj is BlockTableRecord btr)
                {
                    double minX = _position.X, minY = _position.Y, maxX = _position.X, maxY = _position.Y;
                    bool hasEntities = false;

                    foreach (var entId in btr.GetEntityIds())
                    {
                        if (!Database.TryGetObject(entId, out var entObj) || entObj is not Entity ent)
                            continue;

                        hasEntities = true;
                        var ext = ent.GeometricExtents;
                        var corners = new[]
                        {
                            ext.MinPoint,
                            ext.MaxPoint,
                            new Point3d(ext.MinPoint.X, ext.MaxPoint.Y, ext.MinPoint.Z),
                            new Point3d(ext.MaxPoint.X, ext.MinPoint.Y, ext.MinPoint.Z)
                        };

                        foreach (var corner in corners)
                        {
                            var transformed = _blockTransform.TransformPoint(corner);
                            if (transformed.X < minX) minX = transformed.X;
                            if (transformed.Y < minY) minY = transformed.Y;
                            if (transformed.X > maxX) maxX = transformed.X;
                            if (transformed.Y > maxY) maxY = transformed.Y;
                        }
                    }

                    return hasEntities
                        ? new Extents3d(new Point3d(minX, minY, 0), new Point3d(maxX, maxY, 0))
                        : new Extents3d(_position, _position);
                }

                return new Extents3d(_position, _position);
            }
        }

        public override void TransformBy(Matrix3d transform)
        {
            _position = transform.TransformPoint(_position);

            double sx = Math.Sqrt(transform[0, 0] * transform[0, 0] + transform[1, 0] * transform[1, 0]);
            double sy = Math.Sqrt(transform[0, 1] * transform[0, 1] + transform[1, 1] * transform[1, 1]);
            double sz = Math.Abs(transform[2, 2]);

            if (sx > 1e-9)
            {
                double cos = transform[0, 0] / sx;
                double sin = transform[1, 0] / sx;
                _rotation += Math.Atan2(sin, cos);
            }

            _scaleFactors = new Vector3d(
                _scaleFactors.X * sx,
                _scaleFactors.Y * sy,
                _scaleFactors.Z * sz);

            UpdateBlockTransform();
        }

        public override IEnumerable<(Point3d Point, SnapType Type)> GetOsnapPoints()
        {
            yield return (_position, SnapType.Endpoint);

            if (BlockTableRecord.IsNull || Database == null)
                yield break;

            if (!Database.TryGetObject(BlockTableRecord, out var obj) || obj is not BlockTableRecord btr)
                yield break;

            foreach (var entId in btr.GetEntityIds())
            {
                if (!Database.TryGetObject(entId, out var entObj) || entObj is not Entity ent)
                    continue;

                foreach (var (pt, type) in ent.GetOsnapPoints())
                {
                    yield return (_blockTransform.TransformPoint(pt), type);
                }
            }
        }

        public override IEnumerable<Point3d> GetGripPoints()
        {
            yield return _position;
        }

        public override void MoveGripPointsAt(Point3dCollection grips, Vector3d offset)
        {
            if (grips.Count >= 1)
                Position = grips[0] + offset;
        }

        public override IEnumerable<Point3d> GetStretchPoints()
        {
            yield return _position;
        }

        public override void MoveStretchPointsAt(Point3dCollection stretches, Vector3d offset)
        {
            foreach (var pt in stretches)
            {
                if (_position.DistanceTo(pt) < 1e-9)
                {
                    Position = pt + offset;
                    break;
                }
            }
        }

        public override Curve3d? GetGeometricCurve()
        {
            if (BlockTableRecord.IsNull || Database == null)
                return null;

            if (!Database.TryGetObject(BlockTableRecord, out var obj) || obj is not BlockTableRecord btr)
                return null;

            var segments = new List<Curve3d>();
            foreach (var entId in btr.GetEntityIds())
            {
                if (!Database.TryGetObject(entId, out var entObj) || entObj is not Entity ent)
                    continue;

                var copy = ent.GetTransformedCopy(_blockTransform);
                var curve = copy.GetGeometricCurve();
                if (curve != null)
                {
                    if (curve is CompositeCurve3d comp)
                        segments.AddRange(comp.Segments);
                    else
                        segments.Add(curve);
                }
            }
            return segments.Count > 0 ? new CompositeCurve3d(segments) : null;
        }

        public override void List()
        {
            System.Diagnostics.Debug.WriteLine($"                  BlockReference");
            System.Diagnostics.Debug.WriteLine($"Layer: {Layer}");
            System.Diagnostics.Debug.WriteLine($"Position: ({_position.X:F4}, {_position.Y:F4}, {_position.Z:F4})");
            System.Diagnostics.Debug.WriteLine($"Rotation: {Rotation:F4}");
            System.Diagnostics.Debug.WriteLine($"Scale: ({ScaleFactors.X:F4}, {ScaleFactors.Y:F4}, {ScaleFactors.Z:F4})");
        }
    }
}
