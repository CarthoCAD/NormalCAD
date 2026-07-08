using System;
using System.Collections.Generic;
using NormalCAD.Core;
using NormalCAD.Core.ApplicationServices;
using NormalCAD.Core.DatabaseServices;

namespace NormalCAD.Controller.Providers
{
    public class EntityPropertyProvider : IEntityPropertyProvider
    {
        public IEnumerable<PropertyDescriptor> GetProperties(Entity entity)
        {
            yield return new PropertyDescriptor
            {
                Category = "General", DisplayName = "Color", PropertyType = typeof(string),
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
                Category = "General", DisplayName = "Layer", PropertyType = typeof(string),
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
                Category = "General", DisplayName = "Linetype", PropertyType = typeof(string),
                Order = 3, GetValue = () => entity.Linetype,
                ComboValues = new[] { "ByLayer", "ByBlock", "Continuous" },
                TrySetValue = v => { entity.Linetype = (string)v!; return true; }
            };
            yield return new PropertyDescriptor
            {
                Category = "General", DisplayName = "Linetype Scale", PropertyType = typeof(double),
                Order = 4, GetValue = () => entity.LinetypeScale,
                TrySetValue = v => { entity.LinetypeScale = (double)v!; return true; }
            };
            yield return new PropertyDescriptor
            {
                Category = "General", DisplayName = "Lineweight", PropertyType = typeof(LineWeight),
                Order = 5,
                ComboValues = LineWeightFormatter.GetValues(),
                GetValue = () => LineWeightFormatter.Format(entity.LineWeight),
                TrySetValue = v =>
                {
                    if (v is not string s) return false;
                    if (!LineWeightFormatter.TryParse(s, out var lw)) return false;
                    entity.LineWeight = lw;
                    return true;
                }
            };
            yield return new PropertyDescriptor
            {
                Category = "General", DisplayName = "Transparency", PropertyType = typeof(string),
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
