using System;

namespace NormalCAD.Controller.Providers
{
    [AttributeUsage(AttributeTargets.Enum)]
    public sealed class ResourcePrefixAttribute : Attribute
    {
        public string Prefix { get; }

        public ResourcePrefixAttribute(string prefix) => Prefix = prefix;
    }
}
