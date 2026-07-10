using System.Collections.Generic;
using NormalCAD.Core.DatabaseServices;
using NormalCAD.Core.Geometry;
using NormalCAD.Resources;
using NormalCAD.Utilities;

namespace NormalCAD.Controller.Providers
{
    public class ArcPropertyProvider : IEntityPropertyProvider
    {
        private static string CategoryGeometry => EntityPropertyResources.Get("CATEGORY.GEOMETRY");
        private static string StartXLabel => EntityPropertyResources.Get("ARC.GEOMETRY.STARTX");
        private static string StartYLabel => EntityPropertyResources.Get("ARC.GEOMETRY.STARTY");
        private static string StartZLabel => EntityPropertyResources.Get("ARC.GEOMETRY.STARTZ");
        private static string CenterXLabel => EntityPropertyResources.Get("ARC.GEOMETRY.CENTERX");
        private static string CenterYLabel => EntityPropertyResources.Get("ARC.GEOMETRY.CENTERY");
        private static string CenterZLabel => EntityPropertyResources.Get("ARC.GEOMETRY.CENTERZ");
        private static string EndXLabel => EntityPropertyResources.Get("ARC.GEOMETRY.ENDX");
        private static string EndYLabel => EntityPropertyResources.Get("ARC.GEOMETRY.ENDY");
        private static string EndZLabel => EntityPropertyResources.Get("ARC.GEOMETRY.ENDZ");
        private static string RadiusLabel => EntityPropertyResources.Get("ARC.GEOMETRY.RADIUS");
        private static string StartAngleLabel => EntityPropertyResources.Get("ARC.GEOMETRY.STARTANGLE");
        private static string EndAngleLabel => EntityPropertyResources.Get("ARC.GEOMETRY.ENDANGLE");
        private static string TotalAngleLabel => EntityPropertyResources.Get("ARC.GEOMETRY.TOTALANGLE");
        private static string ArcLengthLabel => EntityPropertyResources.Get("ARC.GEOMETRY.ARCLENGTH");
        private static string AreaLabel => EntityPropertyResources.Get("ARC.GEOMETRY.AREA");

        public IEnumerable<PropertyDescriptor> GetProperties(Entity entity)
        {
            if (entity is not Arc arc) yield break;

            yield return new PropertyDescriptor
            {
                Category = CategoryGeometry,
                DisplayName = StartXLabel,
                PropertyType = typeof(double),
                Order = 101,
                GetValue = () => arc.StartPoint.X,
                IsReadOnly = true
            };
            yield return new PropertyDescriptor
            {
                Category = CategoryGeometry,
                DisplayName = StartYLabel,
                PropertyType = typeof(double),
                Order = 102,
                GetValue = () => arc.StartPoint.Y,
                IsReadOnly = true
            };
            yield return new PropertyDescriptor
            {
                Category = CategoryGeometry,
                DisplayName = StartZLabel,
                PropertyType = typeof(double),
                Order = 103,
                GetValue = () => arc.StartPoint.Z,
                IsReadOnly = true
            };
            yield return new PropertyDescriptor
            {
                Category = CategoryGeometry,
                DisplayName = CenterXLabel,
                PropertyType = typeof(double),
                Order = 104,
                GetValue = () => arc.Center.X,
                TrySetValue = v => { arc.Center = new Point3d((double)v!, arc.Center.Y, arc.Center.Z); return true; }
            };
            yield return new PropertyDescriptor
            {
                Category = CategoryGeometry,
                DisplayName = CenterYLabel,
                PropertyType = typeof(double), Order = 105,
                GetValue = () => arc.Center.Y,
                TrySetValue = v => { arc.Center = new Point3d(arc.Center.X, (double)v!, arc.Center.Z); return true; }
            };
            yield return new PropertyDescriptor
            {
                Category = CategoryGeometry,
                DisplayName = CenterZLabel,
                PropertyType = typeof(double),
                Order = 106,
                GetValue = () => arc.Center.Z,
                TrySetValue = v => { arc.Center = new Point3d(arc.Center.X, arc.Center.Y, (double)v!); return true; }
            };
            yield return new PropertyDescriptor
            {
                Category = CategoryGeometry,
                DisplayName = EndXLabel,
                PropertyType = typeof(double),
                Order = 107,
                GetValue = () => arc.EndPoint.X,
                IsReadOnly = true
            };
            yield return new PropertyDescriptor
            {
                Category = CategoryGeometry,
                DisplayName = EndYLabel,
                PropertyType = typeof(double),
                Order = 108,
                GetValue = () => arc.EndPoint.Y,
                IsReadOnly = true
            };
            yield return new PropertyDescriptor
            {
                Category = CategoryGeometry,
                DisplayName = EndZLabel,
                PropertyType = typeof(double),
                Order = 109,
                GetValue = () => arc.EndPoint.Z,
                IsReadOnly = true
            };
            yield return new PropertyDescriptor
            {
                Category = CategoryGeometry,
                DisplayName = RadiusLabel,
                PropertyType = typeof(double),
                Order = 110,
                GetValue = () => arc.Radius,
                TrySetValue = v => { arc.Radius = (double)v!; return true; }
            };
            yield return new PropertyDescriptor
            {
                Category = CategoryGeometry,
                DisplayName = StartAngleLabel,
                PropertyType = typeof(double),
                Order = 111,
                GetValue = () => AngleConverter.ToDegrees(arc.StartAngle),
                TrySetValue = v => { arc.StartAngle = AngleConverter.ToRadians((double)v!); return true; }
            };
            yield return new PropertyDescriptor
            {
                Category = CategoryGeometry,
                DisplayName = EndAngleLabel,
                PropertyType = typeof(double),
                Order = 112,
                GetValue = () => AngleConverter.ToDegrees(arc.EndAngle),
                TrySetValue = v => { arc.EndAngle = AngleConverter.ToRadians((double)v!); return true; }
            };
            yield return new PropertyDescriptor
            {
                Category = CategoryGeometry,
                DisplayName = TotalAngleLabel,
                PropertyType = typeof(double),
                Order = 113,
                GetValue = () => AngleConverter.ToDegrees(arc.TotalAngle),
                IsReadOnly = true
            };
            yield return new PropertyDescriptor
            {
                Category = CategoryGeometry,
                DisplayName = ArcLengthLabel,
                PropertyType = typeof(double),
                Order = 114,
                GetValue = () => arc.Length,
                IsReadOnly = true
            };
            yield return new PropertyDescriptor
            {
                Category = CategoryGeometry,
                DisplayName = AreaLabel,
                PropertyType = typeof(double),
                Order = 115,
                GetValue = () => arc.Area,
                IsReadOnly = true
            };
        }
    }
}
