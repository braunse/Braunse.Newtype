namespace Braunse.Newtype.Test.Unit;

[TestClass]
public class UnitTest1
{
    [TestMethod]
    public void TestMethod1()
    {
        var value1 = new MyNewtypeStruct("hello");
        var value2 = MyNewtypeStruct.Wrap("hello");
        Assert.AreEqual(value1, value2);
    }
}