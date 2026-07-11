namespace NormalCAD.Controller.Providers
{
    public readonly record struct ComboOption(object Value, string DisplayName)
    {
        public override string ToString() => DisplayName;
    }
}
