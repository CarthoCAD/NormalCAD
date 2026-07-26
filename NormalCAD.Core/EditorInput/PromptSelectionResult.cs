using NormalCAD.Core.DatabaseServices;
using NormalCAD.Core.Geometry;

namespace NormalCAD.Core.EditorInput
{
    public struct PromptSelectionResult
    {
        public PromptStatus Status { get; }
        public ObjectIdCollection Value { get; }
        public string StringResult { get; }

        internal PromptSelectionResult(PromptStatus status)
        {
            Status = status;
            Value = new ObjectIdCollection();
            StringResult = string.Empty;
        }

        internal PromptSelectionResult(PromptStatus status,
            ObjectIdCollection value, string stringResult)
        {
            Status = status;
            Value = value;
            StringResult = stringResult;
        }

        public static PromptSelectionResult Cancelled =>
            new(PromptStatus.Cancel);

        public static PromptSelectionResult FromError(string message) =>
            new(PromptStatus.Error, new ObjectIdCollection(), message);

        public static PromptSelectionResult OK(ObjectIdCollection value) =>
            new(PromptStatus.OK, value, "");
    }
}
