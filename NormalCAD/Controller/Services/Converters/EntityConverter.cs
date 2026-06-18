using NormalCAD.Core;
using ACadSharp;
using ACadSharp.Tables;
using CSMath;

namespace NormalCAD.Controller.Services.Converters
{
    public abstract class EntityConverter<TNormal, TAcad> : IConverter
        where TNormal : Entity
        where TAcad : ACadSharp.Entities.Entity
    {
        public bool CanConvertToAcad => true;
        public bool CanConvertToNormal => true;

        public abstract TAcad ConvertToAcad(TNormal source, CadDocument cadDoc);
        public abstract TNormal ConvertToNormal(TAcad source);

        protected static void ApplyLayerAndColorToAcad(TAcad target, TNormal source, CadDocument cadDoc)
        {
            target.Layer = ResolveLayer(source.Layer, cadDoc);
            target.Color = ColorConverter.FromEntityColor(source.Color);
        }

        protected static void ApplyLayerAndColorToNormal(TNormal target, TAcad source)
        {
            target.Layer = source.Layer?.Name ?? "0";
            target.Color = ColorConverter.ToEntityColor(source.Color);
        }

        private static Layer ResolveLayer(string layerName, CadDocument cadDoc)
        {
            if (cadDoc.Layers.TryGetValue(layerName, out var existingLayer))
                return existingLayer;

            var newLayer = new Layer(layerName);
            cadDoc.Layers.Add(newLayer);
            return newLayer;
        }
    }

    public static class ColorConverter
    {
        public static EntityColor ToEntityColor(Color color)
        {
            if (color.IsByLayer)
                return EntityColor.ByLayer;

            return new EntityColor(color.R, color.G, color.B);
        }

        public static Color FromEntityColor(EntityColor color)
        {
            if (color.IsByLayer)
                return Color.ByLayer;

            return new Color(color.R, color.G, color.B);
        }
    }
}
