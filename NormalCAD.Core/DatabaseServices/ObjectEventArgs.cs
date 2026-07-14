using System;

namespace NormalCAD.Core.DatabaseServices
{
    public class ObjectEventArgs : EventArgs
    {
        public DBObject DBObject { get; }

        public ObjectEventArgs(DBObject dbObject)
        {
            DBObject = dbObject;
        }
    }
}
