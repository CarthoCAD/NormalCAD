using System;
using System.Collections.Generic;
using NormalCAD.Core.Geometry;

namespace NormalCAD.Core.DatabaseServices
{
    public class Database : IDisposable
    {
        private long _nextIdValue = 1;
        private readonly Dictionary<ObjectId, DBObject> _objects = new();
        private bool _isDisposed;

        #region Symbol Table IDs

        public ObjectId BlockTableId { get; private set; }
        public ObjectId LayerTableId { get; private set; }
        public ObjectId ViewportTableId { get; private set; }
        public ObjectId LinetypeTableId { get; private set; }
        public ObjectId TextStyleTableId { get; private set; }
        public ObjectId DimStyleTableId { get; private set; }
        public ObjectId RegAppTableId { get; private set; }
        public ObjectId UcsTableId { get; private set; }
        public ObjectId ViewTableId { get; private set; }

        #endregion

        #region Dictionary IDs

        public ObjectId NamedObjectsDictionaryId { get; private set; }
        public ObjectId GroupDictionaryId { get; private set; }
        public ObjectId LayoutDictionaryId { get; private set; }

        #endregion

        #region Structural / State

        public ObjectId CurrentSpaceId { get; set; }
        public ObjectId CurrentViewportTableRecordId { get; set; }
        public int TileMode { get; set; }

        #endregion

        #region File

        public string Filename { get; set; }
        public string OriginalFileName { get; private set; }

        #endregion

        #region Diagnostics

        public int ApproxNumObjects => _objects.Count;
        public bool IsBeingDestroyed => _isDisposed;

        #endregion

        #region Transaction

        public TransactionManager TransactionManager { get; }

        #endregion

        #region Events

        public event EventHandler<ObjectEventArgs>? ObjectAppended;
        public event EventHandler<ObjectEventArgs>? ObjectModified;
        public event EventHandler<ObjectEventArgs>? ObjectErased;

        #endregion

        #region Constructors

        public Database() : this(true, true)
        {
        }

        public Database(bool buildDefaultDrawing, bool noDocument)
        {
            TransactionManager = new TransactionManager(this);
            Filename = "";
            OriginalFileName = "";
            LinetypeTableId = ObjectId.Null;
            TextStyleTableId = ObjectId.Null;
            DimStyleTableId = ObjectId.Null;
            RegAppTableId = ObjectId.Null;
            UcsTableId = ObjectId.Null;
            ViewTableId = ObjectId.Null;
            NamedObjectsDictionaryId = ObjectId.Null;
            GroupDictionaryId = ObjectId.Null;
            LayoutDictionaryId = ObjectId.Null;

            if (buildDefaultDrawing)
            {
                var blockTable = new BlockTable(this);
                BlockTableId = blockTable.ObjectId;

                var layerTable = new LayerTable(this);
                LayerTableId = layerTable.ObjectId;

                var modelSpace = new BlockTableRecord(BlockTableRecord.ModelSpace);
                blockTable.Add(modelSpace);

                var paperSpace = new BlockTableRecord(BlockTableRecord.PaperSpace);
                blockTable.Add(paperSpace);

                CurrentSpaceId = blockTable[BlockTableRecord.ModelSpace];

                var layerZero = new LayerTableRecord("0", EntityColor.White);
                layerTable.Add(layerZero);

                var viewportTable = new ViewportTable(this);
                ViewportTableId = viewportTable.ObjectId;

                var activeViewport = new ViewportTableRecord(
                    ViewportTable.ActiveViewport, Point3d.Origin, 100.0);
                viewportTable.Add(activeViewport);
            }

            if (!noDocument)
            {
                TileMode = 1;
            }
        }

        #endregion

        #region I/O Stubs

        public void ReadDwgFile(string fileName, FileOpenMode mode,
            bool allowCPConversion, string password)
        {
            throw new NotImplementedException();
        }

        public void ReadDxfFile(string fileName)
        {
            throw new NotImplementedException();
        }

        public void DxfIn(string fileName, string? logfileName = null)
        {
            throw new NotImplementedException();
        }

        public void SaveAs(string fileName, bool bBakAndRename,
            DwgVersion version, object? security)
        {
            throw new NotImplementedException();
        }

        public ObjectId Insert(string blockName, Database sourceDb,
            bool preserveSourceDatabase)
        {
            throw new NotImplementedException();
        }

        public ObjectId Insert(Matrix3d transform, Database sourceDb,
            bool preserveSourceDatabase)
        {
            throw new NotImplementedException();
        }

        public void Wblock(Database destDb, ObjectIdCollection ids)
        {
            throw new NotImplementedException();
        }

        public ObjectIdCollection WblockCloneObjects(ObjectIdCollection ids,
            ObjectId ownerId, IdMapping? mapping,
            DuplicateRecordCloning cloning, bool deferXlation)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Object Storage (internal)

        internal DBObject GetObject(ObjectId id)
        {
            if (_objects.TryGetValue(id, out var dbObj))
                return dbObj;
            throw new ArgumentException(
                $"Object with ID {id.Value} not found in database.");
        }

        internal bool TryGetObject(ObjectId id, out DBObject? dbObj)
        {
            return _objects.TryGetValue(id, out dbObj);
        }

        internal ObjectId GenerateNextId()
        {
            return new ObjectId(_nextIdValue++, this);
        }

        internal void RegisterObject(DBObject dbObj)
        {
            _objects[dbObj.ObjectId] = dbObj;
        }

        public bool TryGetObjectId(Handle handle, out ObjectId id)
        {
            id = ObjectId.Null;
            return false;
        }

        #endregion

        #region Event Raisers (internal)

        internal void RaiseObjectAppended(DBObject obj)
        {
            ObjectAppended?.Invoke(this, new ObjectEventArgs(obj));
        }

        internal void RaiseObjectModified(DBObject obj)
        {
            ObjectModified?.Invoke(this, new ObjectEventArgs(obj));
        }

        internal void RaiseObjectErased(DBObject obj)
        {
            ObjectErased?.Invoke(this, new ObjectEventArgs(obj));
        }

        #endregion

        #region Dispose

        public void Dispose()
        {
            if (_isDisposed) return;
            _isDisposed = true;
            _objects.Clear();
        }

        #endregion
    }
}
