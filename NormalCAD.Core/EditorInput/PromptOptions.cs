using System;
using NormalCAD.Core.Geometry;

namespace NormalCAD.Core.EditorInput
{
    public class PromptDistanceOptions : PromptPointOptions
    {
    }

    public class PromptStringOptions
    {
        public string Message { get; set; } = string.Empty;
        public bool AllowSpaces { get; set; }
    }

    public class PromptKeywordOptions
    {
        public string Message { get; set; } = string.Empty;
        public string[] Keywords { get; set; } = Array.Empty<string>();
        public bool AllowArbitraryInput { get; set; }
    }

    public class PromptIntegerOptions
    {
        public string Message { get; set; } = string.Empty;
        public string[] Keywords { get; set; } = Array.Empty<string>();
    }

    public class PromptEntityOptions
    {
        public string Message { get; set; } = string.Empty;
        public string[] Keywords { get; set; } = Array.Empty<string>();
        public bool AllowMultiple { get; set; }
    }

    public class PromptSelectionOptions
    {
        public string Message { get; set; } = string.Empty;
        public string[] Keywords { get; set; } = Array.Empty<string>();
        public Point3d? BasePoint { get; set; }
    }

    public class PromptAngleOptions : PromptPointOptions
    {
        public bool UseDashedLine { get; set; }
    }
}
