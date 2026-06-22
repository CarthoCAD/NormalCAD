using NormalCAD.Core;
using NormalCAD.Core.ApplicationServices;
using NormalCAD.Core.EditorInput;
using DB = NormalCAD.Core.DatabaseServices;

namespace NormalCAD.Host
{
    internal class ApplicationHost : IApplicationHost
    {
        private int _docCounter;

        public DocumentCollection DocumentManager { get; } = new();

        public Document CreateDocument()
        {
            _docCounter++;
            var db = new DB.Database();

            var doc = new Document(db) { Name = $"Drawing{_docCounter}.dwg" };
            doc.Editor = new Editor(doc);

            DocumentManager.Add(doc);
            DocumentManager.SetActive(doc);
            return doc;
        }

        public void ShowAlertDialog(string message)
        {
            System.Diagnostics.Debug.WriteLine($"[NormalCAD] {message}");
        }
    }
}
