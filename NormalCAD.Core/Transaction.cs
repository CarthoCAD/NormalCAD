using System;
using System.Collections.Generic;

namespace NormalCAD.Core.DatabaseServices
{
    public class Transaction : IDisposable
    {
        private readonly Database _database;
        private readonly List<DBObject> _addedObjects = new List<DBObject>();
        private readonly HashSet<DBObject> _modifiedObjects = new HashSet<DBObject>();
        private bool _isCommitted = false;
        private bool _isDisposed = false;

        public Transaction(Database database)
        {
            _database = database;
        }

        public DBObject GetObject(ObjectId id, OpenMode mode)
        {
            var obj = _database.GetObject(id);
            if (mode == OpenMode.ForWrite && !_isCommitted)
            {
                obj.IsModified = true;
                _modifiedObjects.Add(obj);
            }
            return obj;
        }

        public void AddNewlyCreatedDBObject(DBObject obj, bool add)
        {
            if (add)
            {
                _addedObjects.Add(obj);
                _database.RegisterObject(obj);
            }
        }

        public void Commit()
        {
            if (_isCommitted) return;
            _isCommitted = true;

            bool layerModified = false;

            foreach (var obj in _addedObjects)
            {
                obj.IsNewObject = false;
                obj.IsModified = false;
                if (obj is LayerTableRecord)
                    layerModified = true;
            }

            foreach (var obj in _modifiedObjects)
            {
                if (obj is LayerTableRecord)
                    layerModified = true;
            }

            _database.TransactionManager.EndTransaction(this);
            _database.RaiseChanged();

            if (layerModified)
                _database.RaiseLayersChanged();
        }

        public void Abort()
        {
            if (_isCommitted) return;
            
            // Reverter objetos adicionados
            foreach (var obj in _addedObjects)
            {
                if (obj is Entity ent)
                {
                    // Remove do ModelSpace/PaperSpace
                    if (_database.TryGetObject(_database.BlockTableId, out var btObj) && btObj is BlockTable bt)
                    {
                        foreach (var btr in bt)
                        {
                            btr.RemoveEntity(ent.ObjectId);
                        }
                    }
                }
            }
            _database.TransactionManager.EndTransaction(this);
        }

        public void Dispose()
        {
            if (_isDisposed) return;
            _isDisposed = true;
            if (!_isCommitted)
            {
                Abort();
            }
        }
    }
}
