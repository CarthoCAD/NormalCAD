using System.Collections.Generic;
using NormalCAD.Core.DatabaseServices;

namespace NormalCAD.Core
{
    public interface IEntityPropertyProvider
    {
        IEnumerable<PropertyDescriptor> GetProperties(Entity entity);
    }
}
