using System.Collections.Generic;
using NormalCAD.Core.DatabaseServices;

namespace NormalCAD.Controller.Providers
{
    public interface IEntityPropertyProvider
    {
        IEnumerable<PropertyDescriptor> GetProperties(Entity entity);
    }
}
