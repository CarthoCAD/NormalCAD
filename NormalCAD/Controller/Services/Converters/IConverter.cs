using System;

namespace NormalCAD.Controller.Services.Converters
{
    public interface IConverter
    {
        bool CanConvertToAcad { get; }
        bool CanConvertToNormal { get; }
    }
}
