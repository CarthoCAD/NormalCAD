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

            yield return Prop("Geometry", "Center X", typeof(double), 101,
                () => circle.Center.X,
                v => { circle.Center = new Point3d((double)v!, circle.Center.Y, circle.Center.Z); return true; }, false);
            yield return Prop("Geometry", "Center Y", typeof(double), 102,
                () => circle.Center.Y,
                v => { circle.Center = new Point3d(circle.Center.X, (double)v!, circle.Center.Z); return true; }, false);
            yield return Prop("Geometry", "Center Z", typeof(double), 103,
                () => circle.Center.Z,
                v => { circle.Center = new Point3d(circle.Center.X, circle.Center.Y, (double)v!); return true; }, false);
            yield return Prop("Geometry", "Radius", typeof(double), 104,
                () => circle.Radius, v => { circle.Radius = (double)v!; return true; }, false);
            yield return Prop("Geometry", "Diameter", typeof(double), 105,
                () => circle.Diameter, v => { circle.Diameter = (double)v!; return true; }, false);
            yield return Prop("Geometry", "Circumference", typeof(double), 106,
                () => circle.Circumference, v => { circle.Circumference = (double)v!; return true; }, false);
            yield return Prop("Geometry", "Area", typeof(double), 107,
                () => circle.Area, v => { circle.Radius = Math.Sqrt((double)v! / Math.PI); return true; }, false);
        }

        private static PropertyDescriptor Prop(string cat, string name, Type type, int order,
            Func<object?> get, Func<object?, bool>? set, bool readOnly) =>
            new() { Category = cat, DisplayName = name, PropertyType = type, Order = order,
                     GetValue = get, TrySetValue = set, IsReadOnly = readOnly };
    }
}
