using System.Globalization;

namespace NormalCAD.Core
{
    public static class Culture
    {
        public static readonly CultureInfo Invariant = CultureInfo.InvariantCulture;

        public static double Parse(string s) => double.Parse(s, Invariant);
        public static bool TryParse(string s, out double result) =>
            double.TryParse(s, NumberStyles.Float, Invariant, out result);

        public static string ToString(double value) => value.ToString(Invariant);
        public static string ToString(double value, string format) => value.ToString(format, Invariant);
    }
}
