using System;

namespace NormalCAD.Core.EditorInput
{
    public enum PromptStatus
    {
        OK,
        Cancel,
        Keyword,
        Error
    }

    public struct PromptPointResult
    {
        public PromptStatus Status { get; }

        public Geometry.Point3d Value { get; }

        public string StringResult { get; }

        internal PromptPointResult(PromptStatus status)
        {
            Status = status;
            Value = default;
            StringResult = string.Empty;
        }

        internal PromptPointResult(PromptStatus status, Geometry.Point3d value, string stringResult)
        {
            Status = status;
            Value = value;
            StringResult = stringResult;
        }

        public static PromptPointResult Cancelled =>
            new(PromptStatus.Cancel);

        public static PromptPointResult FromError(string message) =>
            new(PromptStatus.Error, default, message);
    }

    public class PromptPointOptions
    {
        public string Message { get; set; } = string.Empty;
        public Geometry.Point3d? BasePoint { get; set; }
        public string[] Keywords { get; set; } = Array.Empty<string>();
        public bool UseBasePoint => BasePoint.HasValue;
    }
}
