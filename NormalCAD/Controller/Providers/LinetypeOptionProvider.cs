using System.Collections.Generic;
using NormalCAD.Resources;

namespace NormalCAD.Controller.Providers
{
    public static class LinetypeOptionProvider
    {
        private static readonly IReadOnlyList<ComboOption> _options;

        static LinetypeOptionProvider()
        {
            _options =
            [
                new ComboOption("ByLayer",    ComboOptionResources.Get("LINETYPE.BYLAYER")),
                new ComboOption("ByBlock",    ComboOptionResources.Get("LINETYPE.BYBLOCK")),
                new ComboOption("Continuous", ComboOptionResources.Get("LINETYPE.CONTINUOUS")),
            ];
        }

        public static IReadOnlyList<ComboOption> GetOptions() => _options;
    }
}
