namespace Braunse.Newtype;

public interface IWrapper<TTo, TFrom>
{
    TFrom Unwrap();
#if NET7_0_OR_GREATER
    static abstract TTo Wrap(TFrom inner);
#endif
}