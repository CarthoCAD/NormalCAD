using System;

namespace NormalCAD.Core.DatabaseServices
{
    public class DBObject : IDisposable
    {
        public ObjectId ObjectId { get; internal set; }
        public Database? Database => ObjectId.Database;
        public ObjectId OwnerId { get; set; }

        public bool IsNewObject { get; internal set; } = true;
        public bool IsModified { get; internal set; } = false;
        public bool IsErased { get; set; }
        public bool IsDisposed { get; private set; }

        public virtual void Dispose()
        {
            IsDisposed = true;
        }
    }
}
