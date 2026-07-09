using System;
using System.Collections.Generic;
using System.Linq;
using NormalCAD.Core.Geometry;

namespace NormalCAD.Core.DatabaseServices
{
    public class Polyline : Curve
    {
        private struct Vertex
        {
            public Point2d Position;
            public double StartWidth;
            public double EndWidth;

            // Bulge defines the arc curvature of the segment that starts at this
            // vertex (0 = straight line). It is stored and round-tripped to/from
            // CAD files, but does NOT yet affect rendering, area/length
            // computation, snapping, osnap points or any other geometry logic —
            // every segment is still treated as a straight line for now.
            public double Bulge;
        }

        private readonly List<Vertex> _vertices = new();
        private bool _closed;

        public double Elevation { get; set; }

        public double Thickness { get; set; }

        public Vector3d Normal { get; set; } = Vector3d.ZAxis;

        public double ConstantWidth { get; set; }

        public int NumberOfVertices => _vertices.Count;

        public bool HasBulges => _vertices.Any(v => v.Bulge != 0.0);

        public bool HasWidth => _vertices.Any(v => v.StartWidth != 0.0 || v.EndWidth != 0.0);

        public override bool Closed
        {
            get => _closed;
            set => _closed = value;
        }

        public override double Length => ComputeLength();

        public override double Area
        {
            get
            {
                if (_vertices.Count < 3) return 0;

                var curve = GetGeometricCurve();
                if (curve is not CompositeCurve3d comp) return 0;

                if (!Closed && EndPoint.DistanceTo(StartPoint) > 1e-9)
                    comp.AddSegment(new LineSegment3d(EndPoint, StartPoint));

                return comp.ComputeEnclosedArea();
            }
        }

        public override Point3d StartPoint =>
            _vertices.Count > 0 ? _vertices[0].Position.ToPoint3d(Elevation) : Point3d.Origin;
        public override Point3d EndPoint =>
            _vertices.Count > 0 ? _vertices[_vertices.Count - 1].Position.ToPoint3d(Elevation) : Point3d.Origin;

        public override Extents3d GeometricExtents
        {
            get
            {
                if (_vertices.Count == 0)
                    return new Extents3d();

                double minX = _vertices[0].Position.X, minY = _vertices[0].Position.Y;
                double maxX = _vertices[0].Position.X, maxY = _vertices[0].Position.Y;
                foreach (var v in _vertices)
                {
                    if (v.Position.X < minX) minX = v.Position.X;
                    if (v.Position.Y < minY) minY = v.Position.Y;
                    if (v.Position.X > maxX) maxX = v.Position.X;
                    if (v.Position.Y > maxY) maxY = v.Position.Y;
                }
                return new Extents3d(
                    new Point3d(minX, minY, Elevation),
                    new Point3d(maxX, maxY, Elevation));
            }
        }

        public Polyline()
        {
        }

        public Polyline(int expectedVertices)
        {
            _vertices.Capacity = expectedVertices;
        }

        public void AddVertexAt(int index, Point2d pt, double bulge, double startWidth, double endWidth)
        {
            _vertices.Insert(index, new Vertex
            {
                Position = pt,
                Bulge = bulge,
                StartWidth = startWidth,
                EndWidth = endWidth
            });
        }

        public void RemoveVertexAt(int index) => _vertices.RemoveAt(index);

        public Point2d GetPoint2dAt(int index) => _vertices[index].Position;

        public Point3d GetPoint3dAt(int index) => _vertices[index].Position.ToPoint3d(Elevation);

        public void SetPointAt(int index, Point2d pt)
        {
            var v = _vertices[index];
            v.Position = pt;
            _vertices[index] = v;
        }

        public double GetStartWidthAt(int index) => _vertices[index].StartWidth;

        public void SetStartWidthAt(int index, double width)
        {
            var v = _vertices[index];
            v.StartWidth = width;
            _vertices[index] = v;
        }

        public double GetEndWidthAt(int index) => _vertices[index].EndWidth;

        public void SetEndWidthAt(int index, double width)
        {
            var v = _vertices[index];
            v.EndWidth = width;
            _vertices[index] = v;
        }

        public void GetWidthsAt(int index, out double startWidth, out double endWidth)
        {
            startWidth = _vertices[index].StartWidth;
            endWidth = _vertices[index].EndWidth;
        }

        public void SetWidthsAt(int index, double startWidth, double endWidth)
        {
            var v = _vertices[index];
            v.StartWidth = startWidth;
            v.EndWidth = endWidth;
            _vertices[index] = v;
        }

        public double GetBulgeAt(int index) => _vertices[index].Bulge;

        public void SetBulgeAt(int index, double bulge)
        {
            var v = _vertices[index];
            v.Bulge = bulge;
            _vertices[index] = v;
        }

        public override Entity Clone()
        {
            var clone = new Polyline(_vertices.Count)
            {
                Elevation = Elevation,
                Thickness = Thickness,
                Normal = Normal,
                ConstantWidth = ConstantWidth,
                Closed = Closed
            };
            clone._vertices.AddRange(_vertices);
            CopyEntityPropertiesTo(clone);
            return clone;
        }

        public override void TransformBy(Matrix3d transform)
        {
            for (int i = 0; i < _vertices.Count; i++)
            {
                var v = _vertices[i];
                var pt3d = transform.TransformPoint(v.Position.ToPoint3d(Elevation));
                v.Position = Point2d.FromPoint3d(pt3d);
                _vertices[i] = v;
            }
            Elevation = transform.TransformPoint(new Point3d(0, 0, Elevation)).Z;
        }

        public override IEnumerable<(Point3d Point, SnapType Type)> GetOsnapPoints()
        {
            for (int i = 0; i < _vertices.Count; i++)
            {
                yield return (_vertices[i].Position.ToPoint3d(Elevation), SnapType.Endpoint);

                if (i < _vertices.Count - 1)
                {
                    var mid = new Point2d(
                        (_vertices[i].Position.X + _vertices[i + 1].Position.X) / 2,
                        (_vertices[i].Position.Y + _vertices[i + 1].Position.Y) / 2);
                    yield return (mid.ToPoint3d(Elevation), SnapType.Midpoint);
                }
            }

            if (Closed && _vertices.Count > 1)
            {
                var mid = new Point2d(
                    (_vertices[_vertices.Count - 1].Position.X + _vertices[0].Position.X) / 2,
                    (_vertices[_vertices.Count - 1].Position.Y + _vertices[0].Position.Y) / 2);
                yield return (mid.ToPoint3d(Elevation), SnapType.Midpoint);
            }
        }

        public override IEnumerable<Point3d> GetGripPoints()
        {
            for (int i = 0; i < _vertices.Count; i++)
                yield return _vertices[i].Position.ToPoint3d(Elevation);
        }

        public override void MoveGripPointsAt(Point3dCollection grips, Vector3d offset)
        {
            for (int i = 0; i < grips.Count && i < _vertices.Count; i++)
            {
                var v = _vertices[i];
                v.Position = Point2d.FromPoint3d(grips[i] + offset);
                _vertices[i] = v;
            }
        }

        public override IEnumerable<Point3d> GetStretchPoints()
        {
            for (int i = 0; i < _vertices.Count; i++)
                yield return _vertices[i].Position.ToPoint3d(Elevation);
        }

        public override void MoveStretchPointsAt(Point3dCollection stretches, Vector3d offset)
        {
            foreach (var pt in stretches)
            {
                for (int i = 0; i < _vertices.Count; i++)
                {
                    if (_vertices[i].Position.ToPoint3d(Elevation).DistanceTo(pt) < 1e-9)
                    {
                        var v = _vertices[i];
                        v.Position = Point2d.FromPoint3d(pt + offset);
                        _vertices[i] = v;
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
                    _vertices[i].Position.ToPoint3d(Elevation),
                    _vertices[j].Position.ToPoint3d(Elevation));
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
                len += _vertices[i].Position.DistanceTo(_vertices[i + 1].Position);
            if (Closed && _vertices.Count > 1)
                len += _vertices[_vertices.Count - 1].Position.DistanceTo(_vertices[0].Position);
            return len;
        }
    }
}
