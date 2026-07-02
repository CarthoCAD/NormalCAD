using System.Resources;

namespace NormalCAD.Resources;

public static class PanelResources
{
    private static readonly ResourceManager _manager =
        new ResourceManager("NormalCAD.Resources.Panels", typeof(PanelResources).Assembly);

    public static string Get(string key)
    {
        return _manager.GetString(key) ?? string.Empty;
    }
}
