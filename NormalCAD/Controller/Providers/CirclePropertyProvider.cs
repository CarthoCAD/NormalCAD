using System;
using System.Collections.Generic;
using NormalCAD.Core.DatabaseServices;
using NormalCAD.Core.Geometry;

namespace NormalCAD.Controller.Providers
{
    public class CirclePropertyProvider : IEntityPropertyProvider
    {
        public IEnumerable<PropertyDescriptor> GetProperties(Entity entity)
        {
            if (entity is not Circle circle) yield break;

            yield return new PropertyDescriptor
            {
                Category = "Geometry",
                DisplayName = "Center X",
                PropertyType = typeof(double),
                Order = 101,
                GetValue = () => circle.Center.X,
                TrySetValue = v => { circle.Center = new Point3d((double)v!, circle.Center.Y, circle.Center.Z); return true; }
            };
            yield return new PropertyDescriptor
            {
                Category = "Geometry",
                DisplayName = "Center Y",
                PropertyType = typeof(double),
                Order = 102,
                GetValue = () => circle.Center.Y,
                TrySetValue = v => { circle.Center = new Point3d(circle.Center.X, (double)v!, circle.Center.Z); return true; }
            };
            yield return new PropertyDescriptor
            {
                Category = "Geometry",
                DisplayName = "Center Z",
                PropertyType = typeof(double),
                Order = 103,
                GetValue = () => circle.Center.Z,
                TrySetValue = v => { circle.Center = new Point3d(circle.Center.X, circle.Center.Y, (double)v!); return true; }
            };
            yield return new PropertyDescriptor
            {
                Category = "Geometry",
                DisplayName = "Radius",
                PropertyType = typeof(double),
                Order = 104,
                GetValue = () => circle.Radius,
                TrySetValue = v => { circle.Radius = (double)v!; return true; }
            };
            yield return new PropertyDescriptor
            {
                Category = "Geometry",
                DisplayName = "Diameter",
                PropertyType = typeof(double),
                Order = 105,
                GetValue = () => circle.Diameter,
                TrySetValue = v => { circle.Diameter = (double)v!; return true; }
            };
            yield return new PropertyDescriptor
            {
                Category = "Geometry",
                DisplayName = "Circumference",
                PropertyType = typeof(double),
                Order = 106,
                GetValue = () => circle.Circumference,
                TrySetValue = v => { circle.Circumference = (double)v!; return true; }
            };
            yield return new PropertyDescriptor
            {
                Category = "Geometry",
                DisplayName = "Area",
                PropertyType = typeof(double),
                Order = 107,
                GetValue = () => circle.Area,
                TrySetValue = v => { circle.Radius = Math.Sqrt((double)v! / Math.PI); return true; }
            };
        }
    }
}
