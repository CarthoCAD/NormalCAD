using System.Resources;

namespace NormalCAD.Resources;

public static class ComboOptionResources
{
    private static readonly ResourceManager _manager =
        new ResourceManager("NormalCAD.Resources.ComboOptions", typeof(ComboOptionResources).Assembly);

    public static string Get(string key)
    {
        return _manager.GetString(key) ?? string.Empty;
    }
}
