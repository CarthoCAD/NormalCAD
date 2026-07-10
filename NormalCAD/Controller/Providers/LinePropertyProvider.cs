using System.Collections.Generic;
using NormalCAD.Core.DatabaseServices;
using NormalCAD.Core.Geometry;
using NormalCAD.Resources;

namespace NormalCAD.Controller.Providers
{
    public class LinePropertyProvider : IEntityPropertyProvider
    {
        private static string CategoryGeometry => EntityPropertyResources.Get("CATEGORY.GEOMETRY");
        private static string StartXLabel => EntityPropertyResources.Get("LINE.GEOMETRY.STARTX");
        private static string StartYLabel => EntityPropertyResources.Get("LINE.GEOMETRY.STARTY");
        private static string StartZLabel => EntityPropertyResources.Get("LINE.GEOMETRY.STARTZ");
        private static string EndXLabel => EntityPropertyResources.Get("LINE.GEOMETRY.ENDX");
        private static string EndYLabel => EntityPropertyResources.Get("LINE.GEOMETRY.ENDY");
        private static string EndZLabel => EntityPropertyResources.Get("LINE.GEOMETRY.ENDZ");
        private static string DeltaXLabel => EntityPropertyResources.Get("LINE.GEOMETRY.DELTAX");
        private static string DeltaYLabel => EntityPropertyResources.Get("LINE.GEOMETRY.DELTAY");
        private static string DeltaZLabel => EntityPropertyResources.Get("LINE.GEOMETRY.DELTAZ");
        private static string LengthLabel => EntityPropertyResources.Get("LINE.GEOMETRY.LENGTH");
        private static string AngleLabel => EntityPropertyResources.Get("LINE.GEOMETRY.ANGLE");

        public IEnumerable<PropertyDescriptor> GetProperties(Entity entity)
        {
            if (entity is not Line line) yield break;

            yield return new PropertyDescriptor
            {
                Category = CategoryGeometry,
                DisplayName = StartXLabel,
                PropertyType = typeof(double),
                Order = 101,
                GetValue = () => line.StartPoint.X,
                TrySetValue = v => { line.StartPoint = new Point3d((double)v!, line.StartPoint.Y, line.StartPoint.Z); return true; }
            };
            yield return new PropertyDescriptor
            {
                Category = CategoryGeometry,
                DisplayName = StartYLabel,
                PropertyType = typeof(double),
                Order = 102,
                GetValue = () => line.StartPoint.Y,
                TrySetValue = v => { line.StartPoint = new Point3d(line.StartPoint.X, (double)v!, line.StartPoint.Z); return true; }
            };
            yield return new PropertyDescriptor
            {
                Category = CategoryGeometry,
                DisplayName = StartZLabel,
                PropertyType = typeof(double),
                Order = 103,
                GetValue = () => line.StartPoint.Z,
                TrySetValue = v => { line.StartPoint = new Point3d(line.StartPoint.X, line.StartPoint.Y, (double)v!); return true; }
            };
            yield return new PropertyDescriptor
            {
                Category = CategoryGeometry,
                DisplayName = EndXLabel,
                PropertyType = typeof(double),
                Order = 104,
                GetValue = () => line.EndPoint.X,
                TrySetValue = v => { line.EndPoint = new Point3d((double)v!, line.EndPoint.Y, line.EndPoint.Z); return true; }
            };
            yield return new PropertyDescriptor
            {
                Category = CategoryGeometry,
                DisplayName = EndYLabel,
                PropertyType = typeof(double),
                Order = 105,
                GetValue = () => line.EndPoint.Y,
                TrySetValue = v => { line.EndPoint = new Point3d(line.EndPoint.X, (double)v!, line.EndPoint.Z); return true; }
            };
            yield return new PropertyDescriptor
            {
                Category = CategoryGeometry,
                DisplayName = EndZLabel,
                PropertyType = typeof(double),
                Order = 106,
                GetValue = () => line.EndPoint.Z,
                TrySetValue = v => { line.EndPoint = new Point3d(line.EndPoint.X, line.EndPoint.Y, (double)v!); return true; }
            };
            yield return new PropertyDescriptor
            {
                Category = CategoryGeometry,
                DisplayName = DeltaXLabel,
                PropertyType = typeof(double),
                Order = 107,
                GetValue = () => line.Delta.X,
                IsReadOnly = true
            };
            yield return new PropertyDescriptor
            {
                Category = CategoryGeometry,
                DisplayName = DeltaYLabel,
                PropertyType = typeof(double),
                Order = 108,
                GetValue = () => line.Delta.Y,
                IsReadOnly = true
            };
            yield return new PropertyDescriptor
            {
                Category = CategoryGeometry,
                DisplayName = DeltaZLabel,
                PropertyType = typeof(double),
                Order = 109,
                GetValue = () => line.Delta.Z,
                IsReadOnly = true
            };
            yield return new PropertyDescriptor
            {
                Category = CategoryGeometry,
                DisplayName = LengthLabel,
                PropertyType = typeof(double),
                Order = 110,
                GetValue = () => line.Length,
                IsReadOnly = true
            };
            yield return new PropertyDescriptor
            {
                Category = CategoryGeometry,
                DisplayName = AngleLabel,
                PropertyType = typeof(double),
                Order = 111,
                GetValue = () => line.Angle,
                IsReadOnly = true
            };
        }
    }
}
