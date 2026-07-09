using System;
using System.Globalization;
using NormalCAD.Core.DatabaseServices;

namespace NormalCAD.Controller.Providers
{
    public static class LineWeightFormatter
    {
        private static readonly string[] _values;
        private static readonly string[] _allValues =
        [
            "ByLayer", "ByBlock", "Default",
            "0.00 mm", "0.05 mm", "0.09 mm", "0.13 mm", "0.15 mm", "0.18 mm",
            "0.20 mm", "0.25 mm", "0.30 mm", "0.35 mm", "0.40 mm", "0.50 mm",
            "0.53 mm", "0.60 mm", "0.70 mm", "0.80 mm", "0.90 mm", "1.00 mm",
            "1.06 mm", "1.20 mm", "1.40 mm", "1.58 mm", "2.00 mm", "2.11 mm"
        ];

        static LineWeightFormatter()
        {
            _values = new string[_allValues.Length];
            Array.Copy(_allValues, _values, _allValues.Length);
        }

        public static string[] GetValues() => _values;
        public static string Format(LineWeight lw)
        {
            return lw switch
            {
                LineWeight.ByLayer => "ByLayer",
                LineWeight.ByBlock => "ByBlock",
                LineWeight.Default => "Default",
                _ => string.Format(CultureInfo.InvariantCulture, "{0:F2} mm", (int)lw / 100.0)
            };
        }

        public static bool TryParse(string s, out LineWeight lw)
        {
            lw = default;
            if (string.IsNullOrWhiteSpace(s)) return false;
            s = s.Trim();

            if (s.Equals("ByLayer", StringComparison.OrdinalIgnoreCase))
            { lw = LineWeight.ByLayer; return true; }
            if (s.Equals("ByBlock", StringComparison.OrdinalIgnoreCase))
            { lw = LineWeight.ByBlock; return true; }
            if (s.Equals("Default", StringComparison.OrdinalIgnoreCase))
            { lw = LineWeight.Default; return true; }

            s = s.Replace("mm", "").Trim();
            if (double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out double mm))
            {
                int value = (int)Math.Round(mm * 100);
                if (Enum.IsDefined(typeof(LineWeight), value))
                {
                    lw = (LineWeight)value;
                    return true;
                }
            }

            return false;
        }
    }
}
