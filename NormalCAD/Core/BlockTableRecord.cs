using System;
using System.Collections.Generic;

namespace NormalCAD.Core
{
    public class BlockTableRecord : SymbolTableRecord
    {
        public const string ModelSpace = "*Model_Space";
        public const string PaperSpace = "*Paper_Space";

        private readonly List<ObjectId> _entityIds = new List<ObjectId>();

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
        }

        public void RemoveEntity(ObjectId entityId)
        {
            _entityIds.Remove(entityId);
        }
    }
}
