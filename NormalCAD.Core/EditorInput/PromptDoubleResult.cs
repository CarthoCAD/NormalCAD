using NormalCAD.Core.Geometry;

namespace NormalCAD.Core.EditorInput
{
    public struct PromptDoubleResult
    {
        public PromptStatus Status { get; }
        public double Value { get; }
        public string StringResult { get; }

        internal PromptDoubleResult(PromptStatus status)
        {
            Status = status;
            Value = 0;
            StringResult = string.Empty;
        }

        internal PromptDoubleResult(PromptStatus status, double value, string stringResult)
        {
            Status = status;
            Value = value;
            StringResult = stringResult;
        }

        public static PromptDoubleResult Cancelled =>
            new(PromptStatus.Cancel);

        public static PromptDoubleResult FromError(string message) =>
            new(PromptStatus.Error, 0, message);
    }
}
