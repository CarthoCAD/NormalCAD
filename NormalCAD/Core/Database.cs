using System;
using System.Collections.Generic;

namespace NormalCAD.Core
{
    public class Database : DBObject
    {
        private long _nextIdValue = 1;
        private readonly Dictionary<ObjectId, DBObject> _objects = new Dictionary<ObjectId, DBObject>();

        public ObjectId BlockTableId { get; private set; }
        public ObjectId LayerTableId { get; private set; }
        public ObjectId ViewportTableId { get; private set; }

        public TransactionManager TransactionManager { get; }

        public Database()
        {
            // O próprio banco de dados se registra
            this.ObjectId = new ObjectId(1, this);
            _objects[this.ObjectId] = this;
            _nextIdValue = 2;

            TransactionManager = new TransactionManager(this);

            // Inicializa as tabelas de símbolos básicas
            var blockTable = new BlockTable(this);
            BlockTableId = blockTable.ObjectId;

            var layerTable = new LayerTable(this);
            LayerTableId = layerTable.ObjectId;

            // Cria os registros padrão das tabelas
            var modelSpace = new BlockTableRecord(BlockTableRecord.ModelSpace);
            blockTable.Add(modelSpace);

            var paperSpace = new BlockTableRecord(BlockTableRecord.PaperSpace);
            blockTable.Add(paperSpace);

            // Cria a camada padrão "0"
            var layerZero = new LayerTableRecord("0", EntityColor.White);
            layerTable.Add(layerZero);

            // Inicializa a tabela de viewports com o viewport *Active
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
