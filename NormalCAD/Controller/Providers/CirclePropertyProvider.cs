using System;
using System.Collections.Generic;
using NormalCAD.Core.DatabaseServices;
using NormalCAD.Core.Geometry;
using NormalCAD.Resources;

namespace NormalCAD.Controller.Providers
{
    public class CirclePropertyProvider : IEntityPropertyProvider
    {
        public string DisplayName => EntityPropertyResources.Get("CIRCLE.DISPLAYNAME");

        private static string CenterXLabel => EntityPropertyResources.Get("CIRCLE.GEOMETRY.CENTERX");
        private static string CenterYLabel => EntityPropertyResources.Get("CIRCLE.GEOMETRY.CENTERY");
        private static string CenterZLabel => EntityPropertyResources.Get("CIRCLE.GEOMETRY.CENTERZ");
        private static string RadiusLabel => EntityPropertyResources.Get("CIRCLE.GEOMETRY.RADIUS");
        private static string DiameterLabel => EntityPropertyResources.Get("CIRCLE.GEOMETRY.DIAMETER");
        private static string CircumferenceLabel => EntityPropertyResources.Get("CIRCLE.GEOMETRY.CIRCUMFERENCE");
        private static string AreaLabel => EntityPropertyResources.Get("CIRCLE.GEOMETRY.AREA");

        public IEnumerable<PropertyDescriptor> GetProperties(Entity entity)
        {
            if (entity is not Circle circle) yield break;

            yield return new PropertyDescriptor
            {
                Category = PropertyCategory.Geometry,
                DisplayName = CenterXLabel,
                PropertyType = typeof(double),
                Order = 101,
                GetValue = () => circle.Center.X,
                TrySetValue = v => { circle.Center = new Point3d((double)v!, circle.Center.Y, circle.Center.Z); return true; }
            };
            yield return new PropertyDescriptor
            {
                Category = PropertyCategory.Geometry,
                DisplayName = CenterYLabel,
                PropertyType = typeof(double),
                Order = 102,
                GetValue = () => circle.Center.Y,
                TrySetValue = v => { circle.Center = new Point3d(circle.Center.X, (double)v!, circle.Center.Z); return true; }
            };
            yield return new PropertyDescriptor
            {
                Category = PropertyCategory.Geometry,
                DisplayName = CenterZLabel,
                PropertyType = typeof(double),
                Order = 103,
                GetValue = () => circle.Center.Z,
                TrySetValue = v => { circle.Center = new Point3d(circle.Center.X, circle.Center.Y, (double)v!); return true; }
            };
            yield return new PropertyDescriptor
            {
                Category = PropertyCategory.Geometry,
                DisplayName = RadiusLabel,
                PropertyType = typeof(double),
                Order = 104,
                GetValue = () => circle.Radius,
                TrySetValue = v => { circle.Radius = (double)v!; return true; }
            };
            yield return new PropertyDescriptor
            {
                Category = PropertyCategory.Geometry,
                DisplayName = DiameterLabel,
                PropertyType = typeof(double),
                Order = 105,
                GetValue = () => circle.Diameter,
                TrySetValue = v => { circle.Diameter = (double)v!; return true; }
            };
            yield return new PropertyDescriptor
            {
                Category = PropertyCategory.Geometry,
                DisplayName = CircumferenceLabel,
                PropertyType = typeof(double),
                Order = 106,
                GetValue = () => circle.Circumference,
                TrySetValue = v => { circle.Circumference = (double)v!; return true; }
            };
            yield return new PropertyDescriptor
            {
                Category = PropertyCategory.Geometry,
                DisplayName = AreaLabel,
                PropertyType = typeof(double),
                Order = 107,
                GetValue = () => circle.Area,
                TrySetValue = v => { circle.Radius = Math.Sqrt((double)v! / Math.PI); return true; }
            };
        }
    }
}
