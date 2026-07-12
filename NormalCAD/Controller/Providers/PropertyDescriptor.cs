using System;
using System.Collections.Generic;
using System.Globalization;

namespace NormalCAD.Controller.Providers
{
    public class PropertyDescriptor
    {
        public PropertyCategory Category { get; init; } = PropertyCategory.General;
        public string DisplayName { get; init; } = string.Empty;
        public Type PropertyType { get; init; } = typeof(string);
        public bool IsReadOnly => TrySetValue == null;
        public int Order { get; init; }
        public Func<object?> GetValue { get; init; } = () => null;
        public Func<object?, bool>? TrySetValue { get; init; }
        public IReadOnlyList<ComboOption>? ComboOptions { get; init; }
        public bool SingleSelectionOnly { get; init; }

        // Value <-> display-string conversion. Kept here (the domain-facing
        // descriptor) as the single place that knows the value's type, so the
        // view only ever deals with strings. This is the extension point that a
        // future units/formatting service (driven by drawing system variables)
        // will back; for now it applies invariant, fixed-precision rules.
        public string Format(object? value)
        {
            if (value is double d)
                return d.ToString("F4", CultureInfo.InvariantCulture);
            return value?.ToString() ?? string.Empty;
        }

        public bool TryParse(string? text, out object? value)
        {
            if (PropertyType == typeof(double))
            {
                if (double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var d))
                {
                    value = d;
                    return true;
                }

                value = null;
                return false;
            }

            if (PropertyType == typeof(int))
            {
                if (int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var i))
                {
                    value = i;
                    return true;
                }

                value = null;
                return false;
            }

            value = text;
            return true;
        }
    }
}
