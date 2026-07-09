using System.Collections.Generic;
using NormalCAD.Core.DatabaseServices;
using NormalCAD.Core.Geometry;

namespace NormalCAD.Controller.Providers
{
    public class LinePropertyProvider : IEntityPropertyProvider
    {
        public IEnumerable<PropertyDescriptor> GetProperties(Entity entity)
        {
            if (entity is not Line line) yield break;

            yield return new PropertyDescriptor
            {
                Category = "Geometry",
                DisplayName = "Start X",
                PropertyType = typeof(double),
                Order = 101,
                GetValue = () => line.StartPoint.X,
                TrySetValue = v => { line.StartPoint = new Point3d((double)v!, line.StartPoint.Y, line.StartPoint.Z); return true; }
            };
            yield return new PropertyDescriptor
            {
                Category = "Geometry",
                DisplayName = "Start Y",
                PropertyType = typeof(double),
                Order = 102,
                GetValue = () => line.StartPoint.Y,
                TrySetValue = v => { line.StartPoint = new Point3d(line.StartPoint.X, (double)v!, line.StartPoint.Z); return true; }
            };
            yield return new PropertyDescriptor
            {
                Category = "Geometry",
                DisplayName = "Start Z",
                PropertyType = typeof(double),
                Order = 103,
                GetValue = () => line.StartPoint.Z,
                TrySetValue = v => { line.StartPoint = new Point3d(line.StartPoint.X, line.StartPoint.Y, (double)v!); return true; }
            };
            yield return new PropertyDescriptor
            {
                Category = "Geometry",
                DisplayName = "End X",
                PropertyType = typeof(double),
                Order = 104,
                GetValue = () => line.EndPoint.X,
                TrySetValue = v => { line.EndPoint = new Point3d((double)v!, line.EndPoint.Y, line.EndPoint.Z); return true; }
            };
            yield return new PropertyDescriptor
            {
                Category = "Geometry",
                DisplayName = "End Y",
                PropertyType = typeof(double),
                Order = 105,
                GetValue = () => line.EndPoint.Y,
                TrySetValue = v => { line.EndPoint = new Point3d(line.EndPoint.X, (double)v!, line.EndPoint.Z); return true; }
            };
            yield return new PropertyDescriptor
            {
                Category = "Geometry",
                DisplayName = "End Z",
                PropertyType = typeof(double),
                Order = 106,
                GetValue = () => line.EndPoint.Z,
                TrySetValue = v => { line.EndPoint = new Point3d(line.EndPoint.X, line.EndPoint.Y, (double)v!); return true; }
            };
            yield return new PropertyDescriptor
            {
                Category = "Geometry",
                DisplayName = "Delta X",
                PropertyType = typeof(double),
                Order = 107,
                GetValue = () => line.Delta.X,
                IsReadOnly = true
            };
            yield return new PropertyDescriptor
            {
                Category = "Geometry",
                DisplayName = "Delta Y",
                PropertyType = typeof(double),
                Order = 108,
                GetValue = () => line.Delta.Y,
                IsReadOnly = true
            };
            yield return new PropertyDescriptor
            {
                Category = "Geometry",
                DisplayName = "Delta Z",
                PropertyType = typeof(double),
                Order = 109,
                GetValue = () => line.Delta.Z,
                IsReadOnly = true
            };
            yield return new PropertyDescriptor
            {
                Category = "Geometry",
                DisplayName = "Length",
                PropertyType = typeof(double),
                Order = 110,
                GetValue = () => line.Length,
                IsReadOnly = true
            };
            yield return new PropertyDescriptor
            {
                Category = "Geometry",
                DisplayName = "Angle",
                PropertyType = typeof(double),
                Order = 111,
                GetValue = () => line.Angle,
                IsReadOnly = true
            };
        }
    }
}
