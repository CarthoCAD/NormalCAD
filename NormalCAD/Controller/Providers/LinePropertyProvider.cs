using System;
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

            yield return Prop("Geometry", "Start X", typeof(double), 101,
                () => line.StartPoint.X,
                v => { line.StartPoint = new Point3d((double)v!, line.StartPoint.Y, line.StartPoint.Z); return true; }, false);
            yield return Prop("Geometry", "Start Y", typeof(double), 102,
                () => line.StartPoint.Y,
                v => { line.StartPoint = new Point3d(line.StartPoint.X, (double)v!, line.StartPoint.Z); return true; }, false);
            yield return Prop("Geometry", "Start Z", typeof(double), 103,
                () => line.StartPoint.Z,
                v => { line.StartPoint = new Point3d(line.StartPoint.X, line.StartPoint.Y, (double)v!); return true; }, false);
            yield return Prop("Geometry", "End X", typeof(double), 104,
                () => line.EndPoint.X,
                v => { line.EndPoint = new Point3d((double)v!, line.EndPoint.Y, line.EndPoint.Z); return true; }, false);
            yield return Prop("Geometry", "End Y", typeof(double), 105,
                () => line.EndPoint.Y,
                v => { line.EndPoint = new Point3d(line.EndPoint.X, (double)v!, line.EndPoint.Z); return true; }, false);
            yield return Prop("Geometry", "End Z", typeof(double), 106,
                () => line.EndPoint.Z,
                v => { line.EndPoint = new Point3d(line.EndPoint.X, line.EndPoint.Y, (double)v!); return true; }, false);
            yield return Prop("Geometry", "Delta X", typeof(double), 107, () => line.Delta.X, null, true);
            yield return Prop("Geometry", "Delta Y", typeof(double), 108, () => line.Delta.Y, null, true);
            yield return Prop("Geometry", "Delta Z", typeof(double), 109, () => line.Delta.Z, null, true);
            yield return Prop("Geometry", "Length", typeof(double), 110, () => line.Length, null, true);
            yield return Prop("Geometry", "Angle", typeof(double), 111, () => line.Angle, null, true);
        }

        private static PropertyDescriptor Prop(string cat, string name, Type type, int order,
            Func<object?> get, Func<object?, bool>? set, bool readOnly) =>
            new() { Category = cat, DisplayName = name, PropertyType = type, Order = order,
                     GetValue = get, TrySetValue = set, IsReadOnly = readOnly };
    }
}
