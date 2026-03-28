using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace Dbx.Tests;

[TestClass]
public class DbxServiceTests
{
    public TestContext TestContext { get; set; }    // MSTest will set this property

    [TestMethod]
    public async Task UpdateItemAsync_WithMissingItem_Logs()
    {
        var id = Path.GetRandomFileName();
        var itemId = "0000111122223333444455556666777700001111222233334444555566667777";
        var logger = new MockLogger<IDbxService>();
        var options = Options.Create(new DbxOptions());
        var json = @"{
""name"": ""John"",
""last"": ""Doe""
}";
        var item = JsonElement.Parse(json);
        var dbx = new DbxService(logger, new HttpContextAccessor(), options);

        var result = await dbx.UpdateItemAsync(id, itemId, item, TestContext.CancellationToken);

        Assert.IsFalse(result.Success);
        Assert.Contains("UpdateItemAsync", logger.LoggedMessage!);
    }

    [TestMethod]
    public async Task CreateListUpdateGetReplaceGetDeleteListItemAsync_AllWorks()
    {
        var id = Path.GetRandomFileName();

        try
        {
            var dbx = BuildService();
            var json = @"{
""name"": ""John"",
""last"": ""Doe""
}";
            var updateJson = @"{""last"": ""Smith""}";
            var item = JsonElement.Parse(json);
            var update = JsonElement.Parse(updateJson);

            // create
            Directory.CreateDirectory(Path.Combine("data", id));
            var result = await dbx.CreateItemAsync(id, item, TestContext.CancellationToken);
            Assert.IsTrue(result.Success);
            Assert.IsNotNull(result.Data);
            string itemId = result.Data.ToString()!;

            // list
            result = await dbx.ListItemsAsync(id, TestContext.CancellationToken);
            Assert.IsNotNull(result.Data);
            Assert.Contains(itemId, (List<string>)result.Data);

            // update
            result = await dbx.UpdateItemAsync(id, itemId, update, TestContext.CancellationToken);
            Assert.IsTrue(result.Success);
            Assert.IsNotNull(result.Data);
            Assert.AreEqual(itemId, result.Data);

            // get
            result = await dbx.GetItemAsync(id, itemId, TestContext.CancellationToken);
            Assert.IsTrue(result.Success);
            Assert.IsNotNull(result.Data);
            Assert.Contains("Smith", result.Data.ToString()!);
            Assert.Contains("John", result.Data.ToString()!);
            Assert.DoesNotContain("Doe", result.Data.ToString()!);

            // replace
            result = await dbx.ReplaceItemAsync(id, itemId, item, TestContext.CancellationToken);
            Assert.IsTrue(result.Success);
            Assert.IsNotNull(result.Data);
            Assert.AreEqual(itemId, result.Data);

            // get
            result = await dbx.GetItemAsync(id, itemId, TestContext.CancellationToken);
            Assert.IsTrue(result.Success);
            Assert.IsNotNull(result.Data);
            Assert.DoesNotContain("Smith", result.Data.ToString()!);
            Assert.Contains("John", result.Data.ToString()!);
            Assert.Contains("Doe", result.Data.ToString()!);

            // delete
            result = await dbx.DeleteItemAsync(id, itemId, TestContext.CancellationToken);
            Assert.IsTrue(result.Success);
            Assert.IsNotNull(result.Data);
            Assert.AreEqual(itemId, result.Data);

            // list
            result = await dbx.ListItemsAsync(id, TestContext.CancellationToken);
            Assert.IsNotNull(result.Data);
            Assert.IsEmpty((List<string>)result.Data);
        }
        finally
        {
            var dir = Path.Combine("data", id);
            if (Directory.Exists(dir))
            {
                Directory.Delete(dir, true);
            }
        }
    }

    [TestMethod]
    public async Task GetItemAsync_WithMissingItem_ReturnsErrorResult()
    {
        var id = Path.GetRandomFileName();
        var dbx = BuildService();
        var itemId = "0000111122223333444455556666777700001111222233334444555566667777";
        var result = await dbx.GetItemAsync(id, itemId, TestContext.CancellationToken);
        Assert.IsFalse(result.Success);
        Assert.IsNotNull(result.Data);
        Assert.Contains(id, result.Data.ToString()!);
        Assert.Contains(itemId, result.Data.ToString()!);
    }

    [TestMethod]
    public async Task ListItemsAsync_CreatesItem()
    {
        var id = Path.GetRandomFileName();
        var dbx = BuildService();
        var result = await dbx.ListItemsAsync(id, TestContext.CancellationToken);

        Assert.IsTrue(result.Success);
        Assert.IsNotNull(result.Data);
        Assert.IsEmpty((string[])result.Data);
    }

    [TestMethod]
    public async Task CreateItemAsync_CreatesItem()
    {
        var id = Path.GetRandomFileName();
        try
        {
            var dbx = BuildService();
            var json = @"{
""name"": ""John"",
""last"": ""Doe""
}";
            var item = JsonElement.Parse(json);
            Directory.CreateDirectory(Path.Combine("data", id));

            var result = await dbx.CreateItemAsync(id, item, TestContext.CancellationToken);

            Assert.IsTrue(result.Success);
            Assert.IsNotNull(result.Data);
        }
        finally
        {
            var dir = Path.Combine("data", id);
            if (Directory.Exists(dir))
            {
                Directory.Delete(dir, true);
            }
        }
    }

    [TestMethod]
    public async Task GetStatusAsync_ReturnsStatus()
    {
        var dbx = BuildService();

        var result = await dbx.GetStatusAsync(TestContext.CancellationToken);

        Assert.IsTrue(result.Success);
    }

    [TestMethod]
    public void CreateWithNullLogger_Throws()
    {
        var options = Options.Create(new DbxOptions());
        var accessor = new HttpContextAccessor();

        var exception = Assert.ThrowsExactly<ArgumentNullException>(() => new DbxService(null!, accessor, options));

        Assert.AreEqual("logger", exception.ParamName);
    }

    [TestMethod]
    public void CreateWithNullAccessor_Throws()
    {
        var logger = NullLogger<IDbxService>.Instance;
        var options = Options.Create(new DbxOptions());

        var exception = Assert.ThrowsExactly<ArgumentNullException>(() => new DbxService(logger, null!, options));

        Assert.AreEqual("httpContextAccessor", exception.ParamName);
    }

    [TestMethod]
    public void CreateWithNullOptions_Throws()
    {
        var logger = NullLogger<IDbxService>.Instance;
        var accessor = new HttpContextAccessor();

        var exception = Assert.ThrowsExactly<ArgumentNullException>(() => new DbxService(logger, accessor, null!));

        Assert.AreEqual("options", exception.ParamName);
    }

    // Creates IDbxService the same way a real ASP.NET Core host would.
    // Use this for tests that validate end-to-end behaviour rather than
    // internal construction details.
    private static IDbxService BuildService(Action<DbxOptions>? configure = null)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDbx(configure);
        var provider = services.BuildServiceProvider();
        return provider.CreateScope().ServiceProvider.GetRequiredService<IDbxService>();
    }

    [TestMethod]
    public void CreateWithNullDataPath_SetsDefaultDataPath()
    {
        var dataPath = "data";
        var logger = NullLogger<IDbxService>.Instance;
        var options = Options.Create(new DbxOptions());
        var accessor = new HttpContextAccessor();

        var service = new DbxService(logger, accessor, options);

        Assert.AreEqual(dataPath, service.DataPath);
    }

    [TestMethod]
    public void CreateWithDataPath_SetsDataPath()
    {
        var dataPath = "data_path";
        var logger = NullLogger<IDbxService>.Instance;
        var options = Options.Create(new DbxOptions() { DataPath = dataPath });
        var accessor = new HttpContextAccessor();

        var service = new DbxService(logger, accessor, options);

        Assert.AreEqual(dataPath, service.DataPath);
    }

    [TestMethod]
    public void CreateWithOptionsAndDataPath_SetsDataPath()
    {
        // Regression: secondary constructor was calling Options.Create(new DbxOptions())
        // instead of Options.Create(options), silently ignoring the provided DataPath.
        var dataPath = "data_path";

        var service = IDbxService.Create(new DbxOptions { DataPath = dataPath });

        Assert.AreEqual(dataPath, ((DbxService)service).DataPath);
    }

    [TestMethod]
    public void CreateWithOptionsAndNullDataPath_SetsDefaultDataPath()
    {
        var service = new DbxService(new DbxOptions());

        Assert.AreEqual("data", service.DataPath);
    }

}
