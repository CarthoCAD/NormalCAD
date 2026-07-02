using System.Resources;

namespace NormalCAD.Resources;

public static class CommandResources
{
    private static readonly ResourceManager _manager =
        new ResourceManager("NormalCAD.Resources.Commands", typeof(CommandResources).Assembly);

    public static string Get(string key)
    {
        return _manager.GetString(key) ?? string.Empty;
    }
}
