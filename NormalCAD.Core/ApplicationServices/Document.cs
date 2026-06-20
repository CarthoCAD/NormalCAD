using System;
using System.Threading;

namespace NormalCAD.Core.ApplicationServices
{
    public class Document
    {
        private readonly object _lock = new();

        public DatabaseServices.Database Database { get; }

        public EditorInput.Editor Editor { get; internal set; }

        public string Name { get; set; }

        public Document(DatabaseServices.Database database)
        {
            Database = database;
            Editor = null!;
            Name = "";
        }

        public DocumentLock LockDocument()
        {
            Monitor.Enter(_lock);
            return new DocumentLock(_lock);
        }
    }
}
