using System;
using System.Collections.Generic;
using System.Globalization;
using NormalCAD.Core.DatabaseServices;
using NormalCAD.Resources;

namespace NormalCAD.Controller.Providers
{
    public static class LineWeightOptionProvider
    {
        private static readonly IReadOnlyList<ComboOption> _options;

        static LineWeightOptionProvider()
        {
            var list = new List<ComboOption>();

            list.Add(new ComboOption(LineWeight.ByLayer, ComboOptionResources.Get("LINEWEIGHT.BYLAYER")));
            list.Add(new ComboOption(LineWeight.ByBlock, ComboOptionResources.Get("LINEWEIGHT.BYBLOCK")));
            list.Add(new ComboOption(LineWeight.Default, ComboOptionResources.Get("LINEWEIGHT.DEFAULT")));

            foreach (LineWeight lw in Enum.GetValues(typeof(LineWeight)))
            {
                if (lw == LineWeight.ByLayer || lw == LineWeight.ByBlock || lw == LineWeight.Default)
                    continue;

                var display = string.Format(CultureInfo.InvariantCulture, "{0:F2} mm", (int)lw / 100.0);
                list.Add(new ComboOption(lw, display));
            }

            _options = list;
        }

        public static IReadOnlyList<ComboOption> GetOptions() => _options;
    }
}
