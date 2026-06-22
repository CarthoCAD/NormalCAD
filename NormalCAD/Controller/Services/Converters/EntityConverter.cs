using NormalCAD.Core.DatabaseServices;
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

        protected static void ApplyEntityPropertiesToAcad(TAcad target, TNormal source, CadDocument cadDoc)
        {
            target.Layer = ResolveLayer(source.Layer, cadDoc);
            target.Color = ColorConverter.FromEntityColor(source.Color);
            target.LineType = ResolveLineType(source.Linetype, cadDoc);
            target.LineTypeScale = source.LinetypeScale;
            target.Transparency = TransparencyConverter.FromNormalTransparency(source.Transparency);
            target.IsInvisible = !source.Visible;
            target.LineWeight = LineWeightConverter.FromNormalLineWeight(source.LineWeight);
        }

        protected static void ApplyEntityPropertiesToNormal(TNormal target, TAcad source)
        {
            target.Layer = source.Layer?.Name ?? "0";
            target.Color = ColorConverter.ToEntityColor(source.Color);
            target.Linetype = source.LineType?.Name ?? "ByLayer";
            target.LinetypeScale = source.LineTypeScale;
            target.Transparency = TransparencyConverter.ToNormalTransparency(source.Transparency);
            target.Visible = !source.IsInvisible;
            target.LineWeight = LineWeightConverter.ToNormalLineWeight(source.LineWeight);
        }

        private static Layer ResolveLayer(string layerName, CadDocument cadDoc)
        {
            if (cadDoc.Layers.TryGetValue(layerName, out var existingLayer))
                return existingLayer;

            var newLayer = new Layer(layerName);
            cadDoc.Layers.Add(newLayer);
            return newLayer;
        }

        private static LineType ResolveLineType(string linetypeName, CadDocument cadDoc)
        {
            if (string.IsNullOrEmpty(linetypeName) || linetypeName == "ByLayer")
                return LineType.ByLayer;
            if (linetypeName == "ByBlock")
                return LineType.ByBlock;
            if (linetypeName == "Continuous")
                return LineType.Continuous;

            if (cadDoc.LineTypes.TryGetValue(linetypeName, out var existing))
                return existing;

            var newLt = new LineType(linetypeName);
            cadDoc.LineTypes.Add(newLt);
            return newLt;
        }
    }

    public static class TransparencyConverter
    {
        public static ACadSharp.Transparency FromNormalTransparency(Core.DatabaseServices.Transparency transparency)
        {
            return new ACadSharp.Transparency(transparency.Alpha);
        }

        public static Core.DatabaseServices.Transparency ToNormalTransparency(ACadSharp.Transparency transparency)
        {
            if (transparency.IsByLayer)
                return Core.DatabaseServices.Transparency.ByLayer;
            return Core.DatabaseServices.Transparency.FromAlpha((byte)transparency.Value);
        }
    }

    public static class LineWeightConverter
    {
        public static LineWeightType FromNormalLineWeight(LineWeight lw)
        {
            return lw switch
            {
                LineWeight.ByLayer => LineWeightType.ByLayer,
                LineWeight.ByBlock => LineWeightType.ByBlock,
                LineWeight.Default => LineWeightType.Default,
                _ => (LineWeightType)((short)lw)
            };
        }

        public static LineWeight ToNormalLineWeight(LineWeightType lw)
        {
            if (lw == LineWeightType.ByLayer) return LineWeight.ByLayer;
            if (lw == LineWeightType.ByBlock) return LineWeight.ByBlock;
            if (lw == LineWeightType.Default) return LineWeight.Default;
            return (LineWeight)(short)lw;
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
