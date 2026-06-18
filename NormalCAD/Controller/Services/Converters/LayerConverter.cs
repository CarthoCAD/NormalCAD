using NormalCAD.Core;
using AcadLayer = ACadSharp.Tables.Layer;

namespace NormalCAD.Controller.Services.Converters
{
    public class LayerConverter : IConverter
    {
        public bool CanConvertToAcad => true;
        public bool CanConvertToNormal => true;

        public AcadLayer ConvertToAcad(LayerTableRecord source)
        {
            return new AcadLayer(source.Name)
            {
                Color = ColorConverter.FromEntityColor(source.Color),
                IsOn = source.IsVisible
            };
        }

        public LayerTableRecord ConvertToNormal(AcadLayer source)
        {
            return new LayerTableRecord(source.Name, ColorConverter.ToEntityColor(source.Color))
            {
                IsVisible = source.IsOn
            };
        }
    }
}
