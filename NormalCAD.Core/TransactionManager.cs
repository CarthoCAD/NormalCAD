using System;
using System.Collections.Generic;

namespace NormalCAD.Core.DatabaseServices
{
    public class TransactionManager
    {
        private readonly Database _database;
        private readonly Stack<Transaction> _transactions = new Stack<Transaction>();

        public TransactionManager(Database database)
        {
            _database = database;
        }

        public Transaction StartTransaction()
        {
            var trans = new Transaction(_database);
            _transactions.Push(trans);
            return trans;
        }

        internal void EndTransaction(Transaction trans)
        {
            if (_transactions.Count > 0 && _transactions.Peek() == trans)
            {
                _transactions.Pop();
            }
        }

        public Transaction TopTransaction
        {
            get
            {
                if (_transactions.Count == 0)
                    throw new InvalidOperationException("No active transaction at this time.");
                return _transactions.Peek();
            }
        }
    }
}
