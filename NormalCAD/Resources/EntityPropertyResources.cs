using System.Resources;

namespace NormalCAD.Resources;

public static class EntityPropertyResources
{
    private static readonly ResourceManager _manager =
        new ResourceManager("NormalCAD.Resources.EntityProperties", typeof(EntityPropertyResources).Assembly);

    public static string Get(string key)
    {
        return _manager.GetString(key) ?? string.Empty;
    }
}
