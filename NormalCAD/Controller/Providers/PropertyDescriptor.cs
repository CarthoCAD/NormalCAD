using System;

namespace NormalCAD.Controller.Providers
{
    public class PropertyDescriptor
    {
        public string Category { get; init; } = string.Empty;
        public string DisplayName { get; init; } = string.Empty;
        public Type PropertyType { get; init; } = typeof(string);
        public bool IsReadOnly { get; init; }
        public int Order { get; init; }
        public Func<object?> GetValue { get; init; } = () => null;
        public Func<object?, bool>? TrySetValue { get; init; }
        public string[]? ComboValues { get; init; }
        public bool SingleSelectionOnly { get; init; }
    }
}
