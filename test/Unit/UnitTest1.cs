namespace Braunse.Newtype.Test.Unit;

[TestClass]
public class UnitTest1
{
    [TestMethod]
    public void TestMethod1()
    {
        var value1 = new MyString("hello");
        var value2 = MyString.Wrap("hello");
        Assert.AreEqual(value1, value2);

        var value3 = new MyInt(42);
        MyInt value4 = 42;
        Assert.IsTrue(value3 == value4);
    }
}