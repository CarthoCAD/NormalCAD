using System.Collections.Generic;
using System.Linq;

namespace NormalCAD.Core.Geometry
{
    public class CompositeCurve3d : Curve3d
    {
        private readonly List<Curve3d> _segments;
        private double[]? _accumLengths;

        public IReadOnlyList<Curve3d> Segments => _segments;

        public override Point3d StartPoint => _segments.Count > 0 ? _segments[0].StartPoint : Point3d.Origin;
        public override Point3d EndPoint => _segments.Count > 0 ? _segments[_segments.Count - 1].EndPoint : Point3d.Origin;
        public override double Length => EnsureAccumLengths().Length > 0 ? _accumLengths![_accumLengths.Length - 1] : 0;

        public double ComputeEnclosedArea()
        {
            if (_segments.Count < 2) return 0;
            double area = 0;
            foreach (var seg in _segments)
                area += seg.GetAreaContribution();
            return System.Math.Abs(area);
        }

        public CompositeCurve3d(IEnumerable<Curve3d> segments)
        {
            _segments = segments.ToList();
        }

        public CompositeCurve3d(params Curve3d[] segments)
        {
            _segments = segments.ToList();
        }

        private double[] EnsureAccumLengths()
        {
            if (_accumLengths == null || _accumLengths.Length != _segments.Count + 1)
            {
                _accumLengths = new double[_segments.Count + 1];
                _accumLengths[0] = 0;
                for (int i = 0; i < _segments.Count; i++)
                    _accumLengths[i + 1] = _accumLengths[i] + _segments[i].Length;
            }
            return _accumLengths;
        }

        private (int segmentIndex, double localDist) LocateDistance(double distance)
        {
            var acc = EnsureAccumLengths();
            double d = System.Math.Max(0, System.Math.Min(acc[acc.Length - 1], distance));
            for (int i = 0; i < _segments.Count; i++)
            {
                if (d <= acc[i + 1])
                    return (i, d - acc[i]);
            }
            return (_segments.Count - 1, _segments[_segments.Count - 1].Length);
        }

        public override Point3d GetPointAtDist(double distance)
        {
            if (_segments.Count == 0) return Point3d.Origin;
            var (idx, localDist) = LocateDistance(distance);
            return _segments[idx].GetPointAtDist(localDist);
        }

        public override double GetDistAtPoint(Point3d point)
        {
            var acc = EnsureAccumLengths();
            var (idx, _) = LocateClosest(point);
            return acc[idx] + _segments[idx].GetDistAtPoint(point);
        }

        public override Point3d GetClosestPointTo(Point3d point)
        {
            return LocateClosestPoint(point);
        }

        public override double GetDistanceTo(Point3d point)
        {
            double minDist = double.MaxValue;
            foreach (var seg in _segments)
            {
                double d = seg.GetDistanceTo(point);
                if (d < minDist) minDist = d;
            }
            return minDist;
        }

        protected override Vector3d GetFirstDerivativeAt(double distance)
        {
            if (_segments.Count == 0) return new Vector3d(0, 0, 0);
            var (idx, localDist) = LocateDistance(distance);
            return _segments[idx].GetFirstDerivative(localDist);
        }

        private Point3d LocateClosestPoint(Point3d point)
        {
            Point3d closest = Point3d.Origin;
            double minDist = double.MaxValue;
            foreach (var seg in _segments)
            {
                var pt = seg.GetClosestPointTo(point);
                double d = pt.DistanceTo(point);
                if (d < minDist)
                {
                    minDist = d;
                    closest = pt;
                }
            }
            return closest;
        }

        private (int segmentIndex, double localDist) LocateClosest(Point3d point)
        {
            int bestIdx = 0;
            double bestDist = double.MaxValue;
            for (int i = 0; i < _segments.Count; i++)
            {
                double d = _segments[i].GetDistanceTo(point);
                if (d < bestDist)
                {
                    bestDist = d;
                    bestIdx = i;
                }
            }
            return (bestIdx, _segments[bestIdx].GetDistAtPoint(point));
        }

        public override void IntersectWith(Curve3d other, Point3dCollection points)
        {
            foreach (var seg in _segments)
                seg.IntersectWith(other, points);
        }
    }
}
