using System.Collections;
using System.Collections.Generic;

namespace NormalCAD.Core.Geometry
{
    public class Point3dCollection : IEnumerable<Point3d>
    {
        private readonly List<Point3d> _points = new();

        public int Count => _points.Count;

        public Point3d this[int index]
        {
            get => _points[index];
            set => _points[index] = value;
        }

        public void Add(Point3d point) => _points.Add(point);

        public void Clear() => _points.Clear();

        public void RemoveAt(int index) => _points.RemoveAt(index);

        public IEnumerator<Point3d> GetEnumerator() => _points.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
