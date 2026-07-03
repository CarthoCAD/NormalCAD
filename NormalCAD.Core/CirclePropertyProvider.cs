using System;
using System.Collections.Generic;
using NormalCAD.Core.DatabaseServices;

namespace NormalCAD.Core
{
    public class CirclePropertyProvider : IEntityPropertyProvider
    {
        public IEnumerable<PropertyDescriptor> GetProperties(Entity entity)
        {
            if (entity is not Circle circle) yield break;

            yield return Prop("Geometry", "Center X", typeof(double), 101, () => circle.CenterX, v => { circle.CenterX = (double)v!; return true; }, false);
            yield return Prop("Geometry", "Center Y", typeof(double), 102, () => circle.CenterY, v => { circle.CenterY = (double)v!; return true; }, false);
            yield return Prop("Geometry", "Radius", typeof(double), 103, () => circle.Radius, v => { circle.Radius = (double)v!; return true; }, false);
            yield return Prop("Geometry", "Length", typeof(double), 104, () => circle.Length, null, true);
            yield return Prop("Geometry", "Closed", typeof(bool), 105, () => circle.Closed, null, true);
        }

        private static PropertyDescriptor Prop(string cat, string name, Type type, int order,
            Func<object?> get, Func<object?, bool>? set, bool readOnly) =>
            new() { Category = cat, DisplayName = name, PropertyType = type, Order = order,
                     GetValue = get, TrySetValue = set, IsReadOnly = readOnly };
    }
}
