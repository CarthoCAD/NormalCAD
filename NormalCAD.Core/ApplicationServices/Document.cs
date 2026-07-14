using System;
using System.Threading;

namespace NormalCAD.Core.ApplicationServices
{
    public class Document
    {
        private readonly object _lock = new();

        public DatabaseServices.Database Database { get; }

        public EditorInput.Editor Editor { get; internal set; }

        public string Name
        {
            get
            {
                var filename = Database.Filename;
                return string.IsNullOrEmpty(filename)
                    ? ""
                    : System.IO.Path.GetFileName(filename);
            }
        }

        public Document(DatabaseServices.Database database)
        {
            Database = database;
            Editor = null!;
        }

        public DocumentLockMode LockMode =>
            Monitor.IsEntered(_lock) ? DocumentLockMode.Write : DocumentLockMode.NotLocked;

        internal bool IsLocked => Monitor.IsEntered(_lock);

        public DocumentLock LockDocument()
        {
            Monitor.Enter(_lock);
            return new DocumentLock(_lock);
        }
    }
}
