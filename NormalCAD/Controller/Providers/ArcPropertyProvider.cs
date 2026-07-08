using System;
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

            yield return Prop("Geometry", "Start X", typeof(double), 101, () => arc.StartPoint.X, null, true);
            yield return Prop("Geometry", "Start Y", typeof(double), 102, () => arc.StartPoint.Y, null, true);
            yield return Prop("Geometry", "Start Z", typeof(double), 103, () => arc.StartPoint.Z, null, true);
            yield return Prop("Geometry", "Center X", typeof(double), 104,
                () => arc.Center.X,
                v => { arc.Center = new Point3d((double)v!, arc.Center.Y, arc.Center.Z); return true; }, false);
            yield return Prop("Geometry", "Center Y", typeof(double), 105,
                () => arc.Center.Y,
                v => { arc.Center = new Point3d(arc.Center.X, (double)v!, arc.Center.Z); return true; }, false);
            yield return Prop("Geometry", "Center Z", typeof(double), 106,
                () => arc.Center.Z,
                v => { arc.Center = new Point3d(arc.Center.X, arc.Center.Y, (double)v!); return true; }, false);
            yield return Prop("Geometry", "End X", typeof(double), 107, () => arc.EndPoint.X, null, true);
            yield return Prop("Geometry", "End Y", typeof(double), 108, () => arc.EndPoint.Y, null, true);
            yield return Prop("Geometry", "End Z", typeof(double), 109, () => arc.EndPoint.Z, null, true);
            yield return Prop("Geometry", "Radius", typeof(double), 110,
                () => arc.Radius, v => { arc.Radius = (double)v!; return true; }, false);
            yield return Prop("Geometry", "Start angle", typeof(double), 111,
                () => AngleConverter.ToDegrees(arc.StartAngle),
                v => { arc.StartAngle = AngleConverter.ToRadians((double)v!); return true; }, false);
            yield return Prop("Geometry", "End angle", typeof(double), 112,
                () => AngleConverter.ToDegrees(arc.EndAngle),
                v => { arc.EndAngle = AngleConverter.ToRadians((double)v!); return true; }, false);
            yield return Prop("Geometry", "Total angle", typeof(double), 113,
                () => AngleConverter.ToDegrees(arc.TotalAngle), null, true);
            yield return Prop("Geometry", "Arc Length", typeof(double), 114,
                () => arc.Length, null, true);
            yield return Prop("Geometry", "Area", typeof(double), 115,
                () => arc.Area, null, true);
        }

        private static PropertyDescriptor Prop(string cat, string name, Type type, int order,
            Func<object?> get, Func<object?, bool>? set, bool readOnly) =>
            new() { Category = cat, DisplayName = name, PropertyType = type, Order = order,
                     GetValue = get, TrySetValue = set, IsReadOnly = readOnly };
    }
}
