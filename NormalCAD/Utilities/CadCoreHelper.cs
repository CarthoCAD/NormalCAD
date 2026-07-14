using System;
using NormalCAD.Core.ApplicationServices;
using NormalCAD.Core.DatabaseServices;

namespace NormalCAD.Utilities
{
    public static class CadCoreHelper
    {
        public static void EditCurrentSpace(Action<Transaction, BlockTableRecord> action)
        {
            var doc = Application.DocumentManager.MdiActiveDocument;
            if (doc == null) return;

            var db = doc.Database;
            var ownLock = doc.LockMode == DocumentLockMode.NotLocked;
            var docLock = ownLock ? doc.LockDocument() : (IDisposable?)null;
            try
            {
                using (var trans = db.TransactionManager.StartTransaction())
                {
                    if (db.CurrentSpaceId.IsNull) return;
                    if (!db.TryGetObject(db.CurrentSpaceId, out var csObj) || csObj is not BlockTableRecord currentSpace)
                        return;

                    action(trans, currentSpace);
                    trans.Commit();
                }
            }
            finally
            {
                docLock?.Dispose();
            }
        }

        public static void AddNewEntityToCurrentSpace(Entity entity)
        {
            EditCurrentSpace((trans, currentSpace) =>
            {
                currentSpace.AppendEntity(entity);
                trans.AddNewlyCreatedDBObject(entity, true);
            });
        }
    }
}
