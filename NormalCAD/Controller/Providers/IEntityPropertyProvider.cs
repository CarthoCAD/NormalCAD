using System.Collections.Generic;
using NormalCAD.Core.DatabaseServices;

namespace NormalCAD.Controller.Providers
{
    public interface IEntityPropertyProvider
    {
        string DisplayName { get; }
        IEnumerable<PropertyDescriptor> GetProperties(Entity entity);
    }
}
