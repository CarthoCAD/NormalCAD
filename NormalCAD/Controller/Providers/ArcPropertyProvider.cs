using System.Collections.Generic;
using NormalCAD.Core.DatabaseServices;
using NormalCAD.Core.Geometry;
using NormalCAD.Utilities;

namespace NormalCAD.Controller.Providers
{
    public class ArcPropertyProvider : IEntityPropertyProvider
    {
        public IEnumerable<PropertyDescriptor> GetProperties(Entity entity)
        {
            if (entity is not Arc arc) yield break;

            yield return new PropertyDescriptor
            {
                Category = "Geometry",
                DisplayName = "Start X",
                PropertyType = typeof(double),
                Order = 101,
                GetValue = () => arc.StartPoint.X,
                IsReadOnly = true
            };
            yield return new PropertyDescriptor
            {
                Category = "Geometry",
                DisplayName = "Start Y",
                PropertyType = typeof(double),
                Order = 102,
                GetValue = () => arc.StartPoint.Y,
                IsReadOnly = true
            };
            yield return new PropertyDescriptor
            {
                Category = "Geometry",
                DisplayName = "Start Z",
                PropertyType = typeof(double),
                Order = 103,
                GetValue = () => arc.StartPoint.Z,
                IsReadOnly = true
            };
            yield return new PropertyDescriptor
            {
                Category = "Geometry",
                DisplayName = "Center X",
                PropertyType = typeof(double),
                Order = 104,
                GetValue = () => arc.Center.X,
                TrySetValue = v => { arc.Center = new Point3d((double)v!, arc.Center.Y, arc.Center.Z); return true; }
            };
            yield return new PropertyDescriptor
            {
                Category = "Geometry",
                DisplayName = "Center Y",
                PropertyType = typeof(double), Order = 105,
                GetValue = () => arc.Center.Y,
                TrySetValue = v => { arc.Center = new Point3d(arc.Center.X, (double)v!, arc.Center.Z); return true; }
            };
            yield return new PropertyDescriptor
            {
                Category = "Geometry",
                DisplayName = "Center Z",
                PropertyType = typeof(double),
                Order = 106,
                GetValue = () => arc.Center.Z,
                TrySetValue = v => { arc.Center = new Point3d(arc.Center.X, arc.Center.Y, (double)v!); return true; }
            };
            yield return new PropertyDescriptor
            {
                Category = "Geometry",
                DisplayName = "End X",
                PropertyType = typeof(double),
                Order = 107,
                GetValue = () => arc.EndPoint.X,
                IsReadOnly = true
            };
            yield return new PropertyDescriptor
            {
                Category = "Geometry",
                DisplayName = "End Y",
                PropertyType = typeof(double),
                Order = 108,
                GetValue = () => arc.EndPoint.Y,
                IsReadOnly = true
            };
            yield return new PropertyDescriptor
            {
                Category = "Geometry",
                DisplayName = "End Z",
                PropertyType = typeof(double),
                Order = 109,
                GetValue = () => arc.EndPoint.Z,
                IsReadOnly = true
            };
            yield return new PropertyDescriptor
            {
                Category = "Geometry",
                DisplayName = "Radius",
                PropertyType = typeof(double),
                Order = 110,
                GetValue = () => arc.Radius,
                TrySetValue = v => { arc.Radius = (double)v!; return true; }
            };
            yield return new PropertyDescriptor
            {
                Category = "Geometry",
                DisplayName = "Start angle",
                PropertyType = typeof(double),
                Order = 111,
                GetValue = () => AngleConverter.ToDegrees(arc.StartAngle),
                TrySetValue = v => { arc.StartAngle = AngleConverter.ToRadians((double)v!); return true; }
            };
            yield return new PropertyDescriptor
            {
                Category = "Geometry",
                DisplayName = "End angle",
                PropertyType = typeof(double),
                Order = 112,
                GetValue = () => AngleConverter.ToDegrees(arc.EndAngle),
                TrySetValue = v => { arc.EndAngle = AngleConverter.ToRadians((double)v!); return true; }
            };
            yield return new PropertyDescriptor
            {
                Category = "Geometry",
                DisplayName = "Total angle",
                PropertyType = typeof(double),
                Order = 113,
                GetValue = () => AngleConverter.ToDegrees(arc.TotalAngle),
                IsReadOnly = true
            };
            yield return new PropertyDescriptor
            {
                Category = "Geometry",
                DisplayName = "Arc Length",
                PropertyType = typeof(double),
                Order = 114,
                GetValue = () => arc.Length,
                IsReadOnly = true
            };
            yield return new PropertyDescriptor
            {
                Category = "Geometry",
                DisplayName = "Area",
                PropertyType = typeof(double),
                Order = 115,
                GetValue = () => arc.Area,
                IsReadOnly = true
            };
        }
    }
}
