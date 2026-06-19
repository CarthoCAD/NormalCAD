using System;
using System.Collections;
using System.Collections.Generic;

namespace NormalCAD.Core
{
    public abstract class SymbolTable : DBObject
    {
        protected SymbolTable(Database database)
        {
            // Associa a tabela ao banco de dados no momento da criação
            this.ObjectId = database.GenerateNextId();
            database.RegisterObject(this);
        }
    }

    public abstract class SymbolTable<T> : SymbolTable, IEnumerable<T> where T : SymbolTableRecord
    {
        private readonly Dictionary<string, ObjectId> _recordsByName = new Dictionary<string, ObjectId>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<ObjectId, T> _recordsById = new Dictionary<ObjectId, T>();

        protected SymbolTable(Database database) : base(database)
        {
        }

        public ObjectId this[string name]
        {
            get
            {
                if (_recordsByName.TryGetValue(name, out var id))
                    return id;
                return ObjectId.Null;
            }
        }

        public bool Has(string name) => _recordsByName.ContainsKey(name);
        public bool Has(ObjectId id) => _recordsById.ContainsKey(id);

        public ObjectId Add(T record)
        {
            if (string.IsNullOrEmpty(record.Name))
                throw new ArgumentException("Record name cannot be empty.");
            if (_recordsByName.ContainsKey(record.Name))
                throw new ArgumentException($"A record with name '{record.Name}' already exists.");

            var db = this.Database ?? throw new InvalidOperationException("SymbolTable is not associated with a database.");
            
            var id = db.GenerateNextId();
            record.ObjectId = id;
            record.IsNewObject = false;

            _recordsByName[record.Name] = id;
            _recordsById[id] = record;
            
            db.RegisterObject(record);

            return id;
        }

        public T GetRecord(ObjectId id)
        {
            if (_recordsById.TryGetValue(id, out var record))
                return record;
            throw new ArgumentException("Record not found.");
        }

        public IEnumerator<T> GetEnumerator() => _recordsById.Values.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
