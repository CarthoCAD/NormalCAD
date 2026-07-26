namespace NormalCAD.Core.EditorInput
{
    public struct PromptKeywordResult
    {
        public PromptStatus Status { get; }
        public string StringResult { get; }

        internal PromptKeywordResult(PromptStatus status)
        {
            Status = status;
            StringResult = string.Empty;
        }

        internal PromptKeywordResult(PromptStatus status, string stringResult)
        {
            Status = status;
            StringResult = stringResult;
        }

        public static PromptKeywordResult Cancelled =>
            new(PromptStatus.Cancel);

        public static PromptKeywordResult FromError(string message) =>
            new(PromptStatus.Error, message);
    }
}
