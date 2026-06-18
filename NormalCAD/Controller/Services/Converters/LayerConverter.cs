using NormalCAD.Core;

namespace NormalCAD.Controller.Services.Converters
{
    public class LayerConverter : IConverter
    {
        public bool CanConvertToAcad => true;
        public bool CanConvertToNormal => true;

        public ACadSharp.Tables.Layer ConvertToAcad(LayerTableRecord source)
        {
            return new ACadSharp.Tables.Layer(source.Name)
            {
                Color = ColorConverter.FromEntityColor(source.Color),
                IsOn = source.IsVisible
            };
        }

        public LayerTableRecord ConvertToNormal(ACadSharp.Tables.Layer source)
        {
            return new LayerTableRecord(source.Name, ColorConverter.ToEntityColor(source.Color))
            {
                IsVisible = source.IsOn
            };
        }
    }
}
