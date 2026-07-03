using System;
using System.Collections.Generic;
using NormalCAD.Core.DatabaseServices;

namespace NormalCAD.Core
{
    public class ArcPropertyProvider : IEntityPropertyProvider
    {
        public IEnumerable<PropertyDescriptor> GetProperties(Entity entity)
        {
            if (entity is not Arc arc) yield break;

            yield return Prop("Geometry", "Center X", typeof(double), 101, () => arc.CenterX, v => { arc.CenterX = (double)v!; return true; }, false);
            yield return Prop("Geometry", "Center Y", typeof(double), 102, () => arc.CenterY, v => { arc.CenterY = (double)v!; return true; }, false);
            yield return Prop("Geometry", "Radius", typeof(double), 103, () => arc.Radius, v => { arc.Radius = (double)v!; return true; }, false);
            yield return Prop("Geometry", "Start Angle", typeof(double), 104, () => arc.StartAngle, v => { arc.StartAngle = (double)v!; return true; }, false);
            yield return Prop("Geometry", "End Angle", typeof(double), 105, () => arc.EndAngle, v => { arc.EndAngle = (double)v!; return true; }, false);
            yield return Prop("Geometry", "Length", typeof(double), 106, () => arc.Length, null, true);
            yield return Prop("Geometry", "Closed", typeof(bool), 107, () => arc.Closed, null, true);
        }

        private static PropertyDescriptor Prop(string cat, string name, Type type, int order,
            Func<object?> get, Func<object?, bool>? set, bool readOnly) =>
            new() { Category = cat, DisplayName = name, PropertyType = type, Order = order,
                     GetValue = get, TrySetValue = set, IsReadOnly = readOnly };
    }
}
