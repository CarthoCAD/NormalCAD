using System.Resources;

namespace NormalCAD.Resources;

public static class DialogResources
{
    private static readonly ResourceManager _manager =
        new ResourceManager("NormalCAD.Resources.Dialogs", typeof(DialogResources).Assembly);

    public static string Get(string key)
    {
        return _manager.GetString(key) ?? string.Empty;
    }
}
