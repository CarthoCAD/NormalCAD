using NormalCAD.Core;
using NormalCAD.Core.ApplicationServices;
using NormalCAD.Core.EditorInput;
using NormalCAD.Resources;
using DB = NormalCAD.Core.DatabaseServices;

namespace NormalCAD.Host
{
    internal class ApplicationHost : IApplicationHost
    {
        private static string DefaultName => DialogResources.Get("APP.DEFAULTNAME");

        private int _docCounter;

        public DocumentCollection DocumentManager { get; } = new();

        public Document CreateDocument()
        {
            _docCounter++;
            var db = new DB.Database(true, false);
            db.Filename = string.Format(DefaultName, _docCounter);

            var doc = new Document(db);
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
