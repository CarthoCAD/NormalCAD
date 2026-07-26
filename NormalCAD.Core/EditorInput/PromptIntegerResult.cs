namespace NormalCAD.Core.EditorInput
{
    public struct PromptIntegerResult
    {
        public PromptStatus Status { get; }
        public int Value { get; }
        public string StringResult { get; }

        internal PromptIntegerResult(PromptStatus status)
        {
            Status = status;
            Value = 0;
            StringResult = string.Empty;
        }

        internal PromptIntegerResult(PromptStatus status, int value, string stringResult)
        {
            Status = status;
            Value = value;
            StringResult = stringResult;
        }

        public static PromptIntegerResult Cancelled =>
            new(PromptStatus.Cancel);

        public static PromptIntegerResult FromError(string message) =>
            new(PromptStatus.Error, 0, message);
    }
}
