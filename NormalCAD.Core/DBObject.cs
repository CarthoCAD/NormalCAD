using System;

namespace NormalCAD.Core.DatabaseServices
{
    public class DBObject : IDisposable
    {
        public ObjectId ObjectId { get; internal set; }
        public Database? Database => ObjectId.Database;
        
        public bool IsNewObject { get; internal set; } = true;
        public bool IsModified { get; internal set; } = false;

        public virtual void Dispose()
        {
        }
    }
}
