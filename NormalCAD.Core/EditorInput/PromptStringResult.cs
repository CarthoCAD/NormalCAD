namespace NormalCAD.Core.EditorInput
{
    public struct PromptStringResult
    {
        public PromptStatus Status { get; }
        public string StringResult { get; }

        internal PromptStringResult(PromptStatus status)
        {
            Status = status;
            StringResult = string.Empty;
        }

        internal PromptStringResult(PromptStatus status, string stringResult)
        {
            Status = status;
            StringResult = stringResult;
        }

        public static PromptStringResult Cancelled =>
            new(PromptStatus.Cancel);

        public static PromptStringResult FromError(string message) =>
            new(PromptStatus.Error, message);
    }
}
