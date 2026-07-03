using System;
using System.Collections.Generic;
using NormalCAD.Core.DatabaseServices;

namespace NormalCAD.Core
{
    public class PolylinePropertyProvider : IEntityPropertyProvider
    {
        public IEnumerable<PropertyDescriptor> GetProperties(Entity entity)
        {
            if (entity is not Polyline polyline) yield break;

            yield return Prop("Geometry", "Elevation", typeof(double), 101, () => polyline.Elevation, v => { polyline.Elevation = (double)v!; return true; }, false);
            yield return Prop("Geometry", "Vertices", typeof(int), 102, () => polyline.NumberOfVertices, null, true);
            yield return Prop("Geometry", "Closed", typeof(bool), 103, () => polyline.Closed, v => { polyline.SetClosed((bool)v!); return true; }, false);
            yield return Prop("Geometry", "Length", typeof(double), 104, () => polyline.Length, null, true);
            yield return Prop("Geometry", "Area", typeof(double), 105, () => polyline.Area, null, true);
        }

        private static PropertyDescriptor Prop(string cat, string name, Type type, int order,
            Func<object?> get, Func<object?, bool>? set, bool readOnly) =>
            new() { Category = cat, DisplayName = name, PropertyType = type, Order = order,
                     GetValue = get, TrySetValue = set, IsReadOnly = readOnly };
    }
}
