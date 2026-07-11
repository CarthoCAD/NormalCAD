using System;
using System.Reflection;
using NormalCAD.Resources;

namespace NormalCAD.Controller.Providers
{
    /// Resolves the localized label of an enum value annotated with
    /// <see cref="ResourcePrefixAttribute"/>. The resource key is built as
    /// "{Prefix}.{MEMBER}" (member name upper-cased), matching the keys in
    /// EntityProperties.resx. Shared by property categories and, later, items.
    public static class LocalizedEnum
    {
        public static string Resolve(Enum value)
        {
            var prefix = value.GetType().GetCustomAttribute<ResourcePrefixAttribute>()?.Prefix;
            var suffix = value.ToString().ToUpperInvariant();
            var key = string.IsNullOrEmpty(prefix) ? suffix : $"{prefix}.{suffix}";
            return EntityPropertyResources.Get(key);
        }
    }
}
