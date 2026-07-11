using System;
using System.Collections.Generic;
using NormalCAD.Core;
using NormalCAD.Core.ApplicationServices;
using NormalCAD.Core.DatabaseServices;
using NormalCAD.Resources;

namespace NormalCAD.Controller.Providers
{
    public class EntityPropertyProvider : IEntityPropertyProvider
    {
        public string DisplayName => string.Empty;

        private static string ColorLabel => EntityPropertyResources.Get("ENTITY.GENERAL.COLOR");
        private static string LayerLabel => EntityPropertyResources.Get("ENTITY.GENERAL.LAYER");
        private static string LinetypeLabel => EntityPropertyResources.Get("ENTITY.GENERAL.LINETYPE");
        private static string LinetypeScaleLabel => EntityPropertyResources.Get("ENTITY.GENERAL.LINETYPESCALE");
        private static string LineweightLabel => EntityPropertyResources.Get("ENTITY.GENERAL.LINEWEIGHT");
        private static string TransparencyLabel => EntityPropertyResources.Get("ENTITY.GENERAL.TRANSPARENCY");

        public IEnumerable<PropertyDescriptor> GetProperties(Entity entity)
        {
            yield return new PropertyDescriptor
            {
                Category = PropertyCategory.General, DisplayName = ColorLabel, PropertyType = typeof(string),
                Order = 1,
                GetValue = () => entity.Color.ToString(),
                TrySetValue = v =>
                {
                    if (v is not string s) return false;
                    if (!EntityColor.TryParse(s, out var c)) return false;
                    entity.Color = c;
                    return true;
                }
            };
            yield return new PropertyDescriptor
            {
                Category = PropertyCategory.General, DisplayName = LayerLabel, PropertyType = typeof(string),
                Order = 2, GetValue = () => entity.Layer,
                TrySetValue = v =>
                {
                    if (v is not string s) return false;
                    var db = Application.DocumentManager.MdiActiveDocument?.Database;
                    if (db != null)
                    {
                        if (!db.TryGetObject(db.LayerTableId, out var ltObj) || ltObj is not LayerTable lt)
                            return false;
                        if (!lt.Has(s))
                            return false;
                    }
                    entity.Layer = s;
                    return true;
                }
            };
            yield return new PropertyDescriptor
            {
                Category = PropertyCategory.General, DisplayName = LinetypeLabel, PropertyType = typeof(string),
                Order = 3, GetValue = () => entity.Linetype,
                ComboOptions = LinetypeOptionProvider.GetOptions(),
                TrySetValue = v => { entity.Linetype = (string)v!; return true; }
            };
            yield return new PropertyDescriptor
            {
                Category = PropertyCategory.General, DisplayName = LinetypeScaleLabel, PropertyType = typeof(double),
                Order = 4, GetValue = () => entity.LinetypeScale,
                TrySetValue = v => { entity.LinetypeScale = (double)v!; return true; }
            };
            yield return new PropertyDescriptor
            {
                Category = PropertyCategory.General, DisplayName = LineweightLabel, PropertyType = typeof(LineWeight),
                Order = 5,
                ComboOptions = LineWeightOptionProvider.GetOptions(),
                GetValue = () => entity.LineWeight,
                TrySetValue = v =>
                {
                    if (v is LineWeight lw) { entity.LineWeight = lw; return true; }
                    return false;
                }
            };
            yield return new PropertyDescriptor
            {
                Category = PropertyCategory.General, DisplayName = TransparencyLabel, PropertyType = typeof(string),
                Order = 6,
                GetValue = () => entity.Transparency.ToString(),
                TrySetValue = v =>
                {
                    if (v is not string s) return false;
                    if (!Transparency.TryParse(s, out var t)) return false;
                    entity.Transparency = t;
                    return true;
                }
            };
        }
    }
}
