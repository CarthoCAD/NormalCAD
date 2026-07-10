using System;
using System.Collections.Generic;

namespace NormalCAD.Controller.Providers
{
    public class PropertyDescriptor
    {
        public string Category { get; init; } = string.Empty;
        public string DisplayName { get; init; } = string.Empty;
        public Type PropertyType { get; init; } = typeof(string);
        public bool IsReadOnly => TrySetValue == null;
        public int Order { get; init; }
        public Func<object?> GetValue { get; init; } = () => null;
        public Func<object?, bool>? TrySetValue { get; init; }
        public IReadOnlyList<ComboOption>? ComboOptions { get; init; }
        public bool SingleSelectionOnly { get; init; }
    }
}
