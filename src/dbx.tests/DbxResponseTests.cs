namespace Dbx.Tests;

[TestClass]
public class DbxResponseTests
{
    [TestMethod]
    public void CreateWithParameters_CreatesRecord()
    {
        var data = "test";
        var success = true;
        var response = new DbxResponse() { Data = data, Success = success };

        Assert.AreEqual(data, response.Data);
        Assert.AreEqual(success, response.Success);
    }
}
