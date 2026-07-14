using System;
using System.Collections.Generic;
using NormalCAD.Core.Geometry;
using NormalCAD.Core.Spatial;

namespace NormalCAD.Core.DatabaseServices
{
    public class BlockTableRecord : SymbolTableRecord
    {
        public const string ModelSpace = "*Model_Space";
        public const string PaperSpace = "*Paper_Space";

        private readonly List<ObjectId> _entityIds = [];
        private readonly RTree _spatialIndex = new();

        public BlockTableRecord()
        {
        }

        public BlockTableRecord(string name)
        {
            Name = name;
        }

        public IEnumerable<ObjectId> GetEntityIds() => _entityIds;

        public void AppendEntity(Entity entity)
        {
            var db = this.Database ?? throw new InvalidOperationException("BlockTableRecord is not associated with a database.");
            
            if (entity.ObjectId.IsNull)
            {
                var id = db.GenerateNextId();
                entity.ObjectId = id;
            }
            
            entity.IsNewObject = false;
            db.RegisterObject(entity);

            _entityIds.Add(entity.ObjectId);
            _spatialIndex.Insert(entity.GeometricExtents, entity.ObjectId);

            db.RaiseObjectAppended(entity);
        }

        public void RemoveEntity(ObjectId entityId)
        {
            if (this.Database != null && this.Database.TryGetObject(entityId, out var obj) && obj is Entity ent)
            {
                _spatialIndex.Remove(ent.GeometricExtents, entityId);
                this.Database.RaiseObjectErased(ent);
            }
            _entityIds.Remove(entityId);
        }

        public IEnumerable<ObjectId> QueryExtents(Extents3d bounds)
        {
            return _spatialIndex.Search(bounds);
        }

        public IEnumerable<ObjectId> QueryNearPoint(Point3d point, double tolerance)
        {
            var queryBounds = new Extents3d(
                new Point3d(point.X - tolerance, point.Y - tolerance, point.Z),
                new Point3d(point.X + tolerance, point.Y + tolerance, point.Z));
            return _spatialIndex.Search(queryBounds);
        }
    }
}
