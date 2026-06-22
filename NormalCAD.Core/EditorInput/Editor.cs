namespace NormalCAD.Core.EditorInput
{
    public class Editor
    {
        public ApplicationServices.Document Document { get; }

        internal Editor(ApplicationServices.Document document)
        {
            Document = document;
        }

        public PromptPointResult GetPoint(string message)
        {
            var options = new PromptPointOptions { Message = message };
            return GetPoint(options);
        }

        public PromptPointResult GetPoint(PromptPointOptions options)
        {
            return PromptPointResult.Cancelled;
        }
    }
}
