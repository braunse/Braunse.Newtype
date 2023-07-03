namespace Braunse.Newtype.Test.Unit;

[Newtype(typeof(int), Comparable = true, ImplicitWrapUnwrap = true)]
public partial struct MyInt {}