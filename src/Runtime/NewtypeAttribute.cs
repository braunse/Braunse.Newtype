namespace Braunse.Newtype;

[AttributeUsage(AttributeTargets.Struct, Inherited = false, AllowMultiple = false)]
public class NewtypeAttribute : Attribute
{
    public NewtypeAttribute(Type inner)
    {
        Inner = inner;
    }

    public Type Inner { get; private set; }
    public bool Convertible { get; set; } = true;
    public bool Opaque { get; set; }
    public bool ImplicitWrapUnwrap { get; set; }
    public bool ImplicitWrap { get; set; }
    public bool ImplicitUnwrap { get; set; }
    public bool Equatable { get; set; }
    public bool Comparable { get; set; }
    public Type? SqlType { get; set; }
}
