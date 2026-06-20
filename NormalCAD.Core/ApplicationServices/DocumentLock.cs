using System;

namespace NormalCAD.Core.ApplicationServices
{
    public struct DocumentLock : IDisposable
    {
        private object? _lockObject;

        internal DocumentLock(object lockObject)
        {
            _lockObject = lockObject;
        }

        public void Dispose()
        {
            if (_lockObject != null)
            {
                System.Threading.Monitor.Exit(_lockObject);
                _lockObject = null;
            }
        }
    }
}
