using System;
using System.Collections.Generic;
using System.Linq;
using NormalCAD.Core.Geometry;

namespace NormalCAD.Core.DatabaseServices
{
    public class Polyline : Curve
    {
        private readonly List<Point2d> _vertices = new();

        public double Elevation { get; set; }
        public int NumberOfVertices => _vertices.Count;

        public Point3d GetPoint3dAt(int index) => _vertices[index].ToPoint3d(Elevation);

        public void AddVertexAt(int index, Point2d pt) => _vertices.Insert(index, pt);

        public void RemoveVertexAt(int index) => _vertices.RemoveAt(index);

        public override Point3d StartPoint =>
            _vertices.Count > 0 ? _vertices[0].ToPoint3d(Elevation) : Point3d.Origin;
        public override Point3d EndPoint =>
            _vertices.Count > 0 ? _vertices[_vertices.Count - 1].ToPoint3d(Elevation) : Point3d.Origin;

        private bool _closed;
        public override bool Closed => _closed;

        public void SetClosed(bool value) => _closed = value;
        public override double Length => ComputeLength();

        public override Extents3d GeometricExtents
        {
            get
            {
                if (_vertices.Count == 0)
                    return new Extents3d();

                double minX = _vertices[0].X, minY = _vertices[0].Y;
                double maxX = _vertices[0].X, maxY = _vertices[0].Y;
                foreach (var v in _vertices)
                {
                    if (v.X < minX) minX = v.X;
                    if (v.Y < minY) minY = v.Y;
                    if (v.X > maxX) maxX = v.X;
                    if (v.Y > maxY) maxY = v.Y;
                }
                return new Extents3d(
                    new Point3d(minX, minY, Elevation),
                    new Point3d(maxX, maxY, Elevation));
            }
        }

        public Polyline()
        {
        }

        public Polyline(IEnumerable<Point2d> vertices, bool closed = false)
        {
            _vertices.AddRange(vertices);
            _closed = closed;
        }

        public override Entity Clone()
        {
            var clone = new Polyline(_vertices, Closed) { Elevation = Elevation };
            CopyEntityPropertiesTo(clone);
            return clone;
        }

        public override void TransformBy(Matrix3d transform)
        {
            for (int i = 0; i < _vertices.Count; i++)
            {
                var pt3d = transform.TransformPoint(_vertices[i].ToPoint3d(Elevation));
                _vertices[i] = Point2d.FromPoint3d(pt3d);
            }
            Elevation = transform.TransformPoint(new Point3d(0, 0, Elevation)).Z;
        }

        public override IEnumerable<(Point3d Point, SnapType Type)> GetOsnapPoints()
        {
            for (int i = 0; i < _vertices.Count; i++)
            {
                yield return (_vertices[i].ToPoint3d(Elevation), SnapType.Endpoint);

                if (i < _vertices.Count - 1)
                {
                    var mid = new Point2d(
                        (_vertices[i].X + _vertices[i + 1].X) / 2,
                        (_vertices[i].Y + _vertices[i + 1].Y) / 2);
                    yield return (mid.ToPoint3d(Elevation), SnapType.Midpoint);
                }
            }

            if (Closed && _vertices.Count > 1)
            {
                var mid = new Point2d(
                    (_vertices[_vertices.Count - 1].X + _vertices[0].X) / 2,
                    (_vertices[_vertices.Count - 1].Y + _vertices[0].Y) / 2);
                yield return (mid.ToPoint3d(Elevation), SnapType.Midpoint);
            }
        }

        public override IEnumerable<Point3d> GetGripPoints()
        {
            for (int i = 0; i < _vertices.Count; i++)
                yield return _vertices[i].ToPoint3d(Elevation);
        }

        public override void MoveGripPointsAt(Point3dCollection grips, Vector3d offset)
        {
            for (int i = 0; i < grips.Count && i < _vertices.Count; i++)
            {
                _vertices[i] = Point2d.FromPoint3d(grips[i] + offset);
            }
        }

        public override IEnumerable<Point3d> GetStretchPoints()
        {
            for (int i = 0; i < _vertices.Count; i++)
                yield return _vertices[i].ToPoint3d(Elevation);
        }

        public override void MoveStretchPointsAt(Point3dCollection stretches, Vector3d offset)
        {
            foreach (var pt in stretches)
            {
                for (int i = 0; i < _vertices.Count; i++)
                {
                    if (_vertices[i].ToPoint3d(Elevation).DistanceTo(pt) < 1e-9)
                    {
                        _vertices[i] = Point2d.FromPoint3d(pt + offset);
                        break;
                    }
                }
            }
        }

        public override Curve3d? GetGeometricCurve()
        {
            int count = Closed ? _vertices.Count : _vertices.Count - 1;
            var segments = new Curve3d[count];
            for (int i = 0; i < count; i++)
            {
                int j = (i + 1) % _vertices.Count;
                segments[i] = new LineSegment3d(
                    _vertices[i].ToPoint3d(Elevation),
                    _vertices[j].ToPoint3d(Elevation));
            }
            return new CompositeCurve3d(segments);
        }

        public override void List()
        {
            System.Diagnostics.Debug.WriteLine($"                  Polyline");
            System.Diagnostics.Debug.WriteLine($"Layer: {Layer}");
            System.Diagnostics.Debug.WriteLine($"Vertices: {NumberOfVertices}");
            System.Diagnostics.Debug.WriteLine($"Closed: {Closed}");
            System.Diagnostics.Debug.WriteLine($"Length: {Length:F4}");
        }

        private double ComputeLength()
        {
            double len = 0;
            for (int i = 0; i < _vertices.Count - 1; i++)
                len += _vertices[i].DistanceTo(_vertices[i + 1]);
            if (Closed && _vertices.Count > 1)
                len += _vertices[_vertices.Count - 1].DistanceTo(_vertices[0]);
            return len;
        }
    }
}
