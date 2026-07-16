using NormalCAD.Core.DatabaseServices;

namespace NormalCAD.Core.EditorInput
{
    public struct PromptEntityResult
    {
        public PromptStatus Status { get; }
        public ObjectId ObjectId { get; }
        public string StringResult { get; }

        internal PromptEntityResult(PromptStatus status)
        {
            Status = status;
            ObjectId = ObjectId.Null;
            StringResult = string.Empty;
        }

        internal PromptEntityResult(PromptStatus status, ObjectId objectId, string stringResult)
        {
            Status = status;
            ObjectId = objectId;
            StringResult = stringResult;
        }

        public static PromptEntityResult Cancelled =>
            new(PromptStatus.Cancel);

        public static PromptEntityResult FromError(string message) =>
            new(PromptStatus.Error, ObjectId.Null, message);
    }
}
