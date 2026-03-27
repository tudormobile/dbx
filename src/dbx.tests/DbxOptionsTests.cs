namespace Dbx.Tests;

[TestClass]
public class DbxOptionsTests
{
    [TestMethod]
    public void DefaultConstructor_Constructs()
    {
        var options = new DbxOptions();

        Assert.IsNull(options.DataPath);
        Assert.IsNull(options.Prefix);
    }

    [TestMethod]
    public void ConstructorWithParameters_Constructs()
    {
        var dataPath = "Some data path";
        var prefix = "some prefix";

        var options = new DbxOptions()
        {
            DataPath = dataPath,
            Prefix = prefix
        };

        Assert.AreEqual(dataPath, options.DataPath);
        Assert.AreEqual(prefix, options.Prefix);
    }

}
