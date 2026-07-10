using System;
using System.Collections.Generic;
using NormalCAD.Core.DatabaseServices;
using NormalCAD.Core.Geometry;
using NormalCAD.Resources;

namespace NormalCAD.Controller.Providers
{
    public class PolylinePropertyProvider : IEntityPropertyProvider
    {
        public string DisplayName => EntityPropertyResources.Get("POLYLINE.DISPLAYNAME");

        private static string CategoryGeometry => EntityPropertyResources.Get("CATEGORY.GEOMETRY");
        private static string CategoryMisc => EntityPropertyResources.Get("CATEGORY.MISC");
        private static string CurrentVertexLabel => EntityPropertyResources.Get("POLYLINE.GEOMETRY.CURRENTVERTEX");
        private static string VertexXLabel => EntityPropertyResources.Get("POLYLINE.GEOMETRY.VERTEXX");
        private static string VertexYLabel => EntityPropertyResources.Get("POLYLINE.GEOMETRY.VERTEXY");
        private static string StartSegmentWidthLabel => EntityPropertyResources.Get("POLYLINE.GEOMETRY.STARTSEGMENTWIDTH");
        private static string EndSegmentWidthLabel => EntityPropertyResources.Get("POLYLINE.GEOMETRY.ENDSEGMENTWIDTH");
        private static string GlobalWidthLabel => EntityPropertyResources.Get("POLYLINE.GEOMETRY.GLOBALWIDTH");
        private static string ElevationLabel => EntityPropertyResources.Get("POLYLINE.GEOMETRY.ELEVATION");
        private static string AreaLabel => EntityPropertyResources.Get("POLYLINE.GEOMETRY.AREA");
        private static string LengthLabel => EntityPropertyResources.Get("POLYLINE.GEOMETRY.LENGTH");
        private static string ClosedLabel => EntityPropertyResources.Get("POLYLINE.MISC.CLOSED");

        private Polyline? _current;
        private int _vertexIndex;

        public IEnumerable<PropertyDescriptor> GetProperties(Entity entity)
        {
            if (entity is not Polyline polyline) yield break;

            if (!ReferenceEquals(_current, polyline))
            {
                _current = polyline;
                _vertexIndex = 0;
            }

            _vertexIndex = Clamp(_vertexIndex, polyline.NumberOfVertices);
            bool hasVertex = polyline.NumberOfVertices > 0;

            yield return new PropertyDescriptor
            {
                Category = CategoryGeometry,
                DisplayName = CurrentVertexLabel,
                PropertyType = typeof(int),
                Order = 101,
                SingleSelectionOnly = true,
                GetValue = () => _vertexIndex + 1,
                TrySetValue = v =>
                {
                    if (polyline.NumberOfVertices == 0) return false;
                    int idx = (int)v! - 1;
                    if (idx < 0 || idx >= polyline.NumberOfVertices) return false;
                    _vertexIndex = idx;
                    return true;
                }
            };
            yield return new PropertyDescriptor
            {
                Category = CategoryGeometry,
                DisplayName = VertexXLabel,
                PropertyType = typeof(double),
                Order = 102,
                SingleSelectionOnly = true,
                GetValue = () => hasVertex ? polyline.GetPoint2dAt(_vertexIndex).X : 0.0,
                TrySetValue = v =>
                {
                    if (!hasVertex) return false;
                    var p = polyline.GetPoint2dAt(_vertexIndex);
                    polyline.SetPointAt(_vertexIndex, new Point2d((double)v!, p.Y));
                    return true;
                }
            };
            yield return new PropertyDescriptor
            {
                Category = CategoryGeometry,
                DisplayName = VertexYLabel,
                PropertyType = typeof(double),
                Order = 103,
                SingleSelectionOnly = true,
                GetValue = () => hasVertex ? polyline.GetPoint2dAt(_vertexIndex).Y : 0.0,
                TrySetValue = v =>
                {
                    if (!hasVertex) return false;
                    var p = polyline.GetPoint2dAt(_vertexIndex);
                    polyline.SetPointAt(_vertexIndex, new Point2d(p.X, (double)v!));
                    return true;
                }
            };
            yield return new PropertyDescriptor
            {
                Category = CategoryGeometry,
                DisplayName = StartSegmentWidthLabel,
                PropertyType = typeof(double),
                Order = 104,
                SingleSelectionOnly = true,
                GetValue = () => hasVertex ? polyline.GetStartWidthAt(_vertexIndex) : 0.0,
                TrySetValue = v =>
                {
                    if (!hasVertex) return false;
                    polyline.SetStartWidthAt(_vertexIndex, (double)v!);
                    return true;
                }
            };
            yield return new PropertyDescriptor
            {
                Category = CategoryGeometry,
                DisplayName = EndSegmentWidthLabel,
                PropertyType = typeof(double),
                Order = 105,
                SingleSelectionOnly = true,
                GetValue = () => hasVertex ? polyline.GetEndWidthAt(_vertexIndex) : 0.0,
                TrySetValue = v =>
                {
                    if (!hasVertex) return false;
                    polyline.SetEndWidthAt(_vertexIndex, (double)v!);
                    return true;
                }
            };
            yield return new PropertyDescriptor
            {
                Category = CategoryGeometry,
                DisplayName = GlobalWidthLabel,
                PropertyType = typeof(double),
                Order = 106,
                GetValue = () => GetGlobalWidth(polyline),
                TrySetValue = v =>
                {
                    double width = (double)v!;
                    for (int i = 0; i < polyline.NumberOfVertices; i++)
                        polyline.SetWidthsAt(i, width, width);
                    return true;
                }
            };
            yield return new PropertyDescriptor
            {
                Category = CategoryGeometry,
                DisplayName = ElevationLabel,
                PropertyType = typeof(double),
                Order = 107,
                GetValue = () => polyline.Elevation,
                TrySetValue = v => { polyline.Elevation = (double)v!; return true; }
            };
            yield return new PropertyDescriptor
            {
                Category = CategoryGeometry,
                DisplayName = AreaLabel,
                PropertyType = typeof(double),
                Order = 108,
                GetValue = () => polyline.Area,
            };
            yield return new PropertyDescriptor
            {
                Category = CategoryGeometry,
                DisplayName = LengthLabel,
                PropertyType = typeof(double),
                Order = 109,
                GetValue = () => polyline.Length
            };
            yield return new PropertyDescriptor
            {
                Category = CategoryMisc,
                DisplayName = ClosedLabel,
                PropertyType = typeof(bool),
                Order = 201,
                GetValue = () => polyline.Closed,
                TrySetValue = v => { polyline.Closed = (bool)v!; return true; }
            };
        }

        private static object GetGlobalWidth(Polyline polyline)
        {
            if (polyline.NumberOfVertices == 0) return "";

            polyline.GetWidthsAt(0, out double start, out _);
            double reference = start;
            for (int i = 0; i < polyline.NumberOfVertices; i++)
            {
                polyline.GetWidthsAt(i, out double s, out double e);
                if (Math.Abs(s - reference) > 1e-9 || Math.Abs(e - reference) > 1e-9)
                    return "";
            }
            return reference;
        }

        private static int Clamp(int index, int count)
        {
            if (count == 0) return 0;
            if (index < 0) return 0;
            if (index >= count) return count - 1;
            return index;
        }
    }
}
