using System;
using System.Collections.Generic;

namespace NormalCAD.Core.DatabaseServices
{
    public class Database : DBObject
    {
        private long _nextIdValue = 1;
        private readonly Dictionary<ObjectId, DBObject> _objects = new Dictionary<ObjectId, DBObject>();

        public ObjectId BlockTableId { get; private set; }
        public ObjectId LayerTableId { get; private set; }
        public ObjectId ViewportTableId { get; private set; }

        public TransactionManager TransactionManager { get; }

        public event Action? Changed;
        public event Action? LayersChanged;

        public Database()
        {
            // The database registers itself
            this.ObjectId = new ObjectId(1, this);
            _objects[this.ObjectId] = this;
            _nextIdValue = 2;

            TransactionManager = new TransactionManager(this);

            // Initialize basic symbol tables
            var blockTable = new BlockTable(this);
            BlockTableId = blockTable.ObjectId;

            var layerTable = new LayerTable(this);
            LayerTableId = layerTable.ObjectId;

            // Create default table records
            var modelSpace = new BlockTableRecord(BlockTableRecord.ModelSpace);
            blockTable.Add(modelSpace);

            var paperSpace = new BlockTableRecord(BlockTableRecord.PaperSpace);
            blockTable.Add(paperSpace);

            // Create default layer "0"
            var layerZero = new LayerTableRecord("0", EntityColor.White);
            layerTable.Add(layerZero);

            // Initialize viewport table with *Active viewport
            var viewportTable = new ViewportTable(this);
            ViewportTableId = viewportTable.ObjectId;

            var activeViewport = new ViewportTableRecord(ViewportTable.ActiveViewport, Geometry.Point3d.Origin, 100.0);
            viewportTable.Add(activeViewport);
        }

        internal ObjectId GenerateNextId()
        {
            return new ObjectId(_nextIdValue++, this);
        }

        internal void RegisterObject(DBObject dbObj)
        {
            _objects[dbObj.ObjectId] = dbObj;
        }

        internal void RaiseChanged()
        {
            Changed?.Invoke();
        }

        internal void RaiseLayersChanged()
        {
            LayersChanged?.Invoke();
        }

        public DBObject GetObject(ObjectId id)
        {
            if (_objects.TryGetValue(id, out var dbObj))
                return dbObj;
            throw new ArgumentException($"Object with ID {id.Value} not found in database.");
        }

        public bool TryGetObject(ObjectId id, out DBObject? dbObj)
        {
            return _objects.TryGetValue(id, out dbObj);
        }
    }
}
