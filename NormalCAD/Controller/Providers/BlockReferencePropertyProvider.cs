using System.Collections.Generic;
using NormalCAD.Core.DatabaseServices;
using NormalCAD.Core.Geometry;
using NormalCAD.Resources;
using NormalCAD.Utilities;

namespace NormalCAD.Controller.Providers
{
    public class BlockReferencePropertyProvider : IEntityPropertyProvider
    {
        public string DisplayName => EntityPropertyResources.Get("BLOCKREFERENCE.DISPLAYNAME");

        private static string PositionXLabel => EntityPropertyResources.Get("BLOCKREFERENCE.GEOMETRY.POSITIONX");
        private static string PositionYLabel => EntityPropertyResources.Get("BLOCKREFERENCE.GEOMETRY.POSITIONY");
        private static string PositionZLabel => EntityPropertyResources.Get("BLOCKREFERENCE.GEOMETRY.POSITIONZ");
        private static string ScaleXLabel => EntityPropertyResources.Get("BLOCKREFERENCE.GEOMETRY.SCALEX");
        private static string ScaleYLabel => EntityPropertyResources.Get("BLOCKREFERENCE.GEOMETRY.SCALEY");
        private static string ScaleZLabel => EntityPropertyResources.Get("BLOCKREFERENCE.GEOMETRY.SCALEZ");
        private static string NameLabel => EntityPropertyResources.Get("BLOCKREFERENCE.MISC.NAME");
        private static string RotationLabel => EntityPropertyResources.Get("BLOCKREFERENCE.MISC.ROTATION");

        public IEnumerable<PropertyDescriptor> GetProperties(Entity entity)
        {
            if (entity is not BlockReference br) yield break;

            yield return new PropertyDescriptor
            {
                Category = PropertyCategory.Geometry,
                DisplayName = PositionXLabel,
                PropertyType = typeof(double),
                Order = 101,
                GetValue = () => br.Position.X,
                TrySetValue = v => { br.Position = new Point3d((double)v!, br.Position.Y, br.Position.Z); return true; }
            };
            yield return new PropertyDescriptor
            {
                Category = PropertyCategory.Geometry,
                DisplayName = PositionYLabel,
                PropertyType = typeof(double),
                Order = 102,
                GetValue = () => br.Position.Y,
                TrySetValue = v => { br.Position = new Point3d(br.Position.X, (double)v!, br.Position.Z); return true; }
            };
            yield return new PropertyDescriptor
            {
                Category = PropertyCategory.Geometry,
                DisplayName = PositionZLabel,
                PropertyType = typeof(double),
                Order = 103,
                GetValue = () => br.Position.Z,
                TrySetValue = v => { br.Position = new Point3d(br.Position.X, br.Position.Y, (double)v!); return true; }
            };
            yield return new PropertyDescriptor
            {
                Category = PropertyCategory.Geometry,
                DisplayName = ScaleXLabel,
                PropertyType = typeof(double),
                Order = 104,
                GetValue = () => br.ScaleFactors.X,
                TrySetValue = v => { br.ScaleFactors = new Vector3d((double)v!, br.ScaleFactors.Y, br.ScaleFactors.Z); return true; }
            };
            yield return new PropertyDescriptor
            {
                Category = PropertyCategory.Geometry,
                DisplayName = ScaleYLabel,
                PropertyType = typeof(double),
                Order = 105,
                GetValue = () => br.ScaleFactors.Y,
                TrySetValue = v => { br.ScaleFactors = new Vector3d(br.ScaleFactors.X, (double)v!, br.ScaleFactors.Z); return true; }
            };
            yield return new PropertyDescriptor
            {
                Category = PropertyCategory.Geometry,
                DisplayName = ScaleZLabel,
                PropertyType = typeof(double),
                Order = 106,
                GetValue = () => br.ScaleFactors.Z,
                TrySetValue = v => { br.ScaleFactors = new Vector3d(br.ScaleFactors.X, br.ScaleFactors.Y, (double)v!); return true; }
            };
            yield return new PropertyDescriptor
            {
                Category = PropertyCategory.Misc,
                DisplayName = NameLabel,
                PropertyType = typeof(string),
                Order = 107,
                GetValue = () => br.Name,
            };
            yield return new PropertyDescriptor
            {
                Category = PropertyCategory.Misc,
                DisplayName = RotationLabel,
                PropertyType = typeof(double),
                Order = 108,
                GetValue = () => AngleConverter.ToDegrees(br.Rotation),
                TrySetValue = v => { br.Rotation = AngleConverter.ToRadians((double)v!); return true; }
            };
        }
    }
}
