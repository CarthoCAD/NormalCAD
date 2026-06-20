using System.Collections.Generic;

namespace NormalCAD.Core.ApplicationServices
{
    public class DocumentCollection
    {
        private readonly List<Document> _documents = new();

        public Document? MdiActiveDocument { get; internal set; }

        public int Count => _documents.Count;

        public void Add(Document document)
        {
            _documents.Add(document);
            if (MdiActiveDocument == null)
                MdiActiveDocument = document;
        }

        public void SetActive(Document document)
        {
            if (_documents.Contains(document))
                MdiActiveDocument = document;
        }
    }
}
