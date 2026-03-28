using System.Text.Json;

namespace Dbx.Tests;

[TestClass]
public class DbxRepositoryTests
{
    [TestMethod]
    public void Constructor_WithValidDataPath_Constructs()
    {
        var dataPath = "some_path";
        var repository = new DbxRepository(dataPath);
        Assert.IsNotNull(repository);
    }

    [TestMethod]
    public void Constructor_WithWhitespace_Throws()
    {
        var dataPath = "  ";
        var exception = Assert.ThrowsExactly<ArgumentException>(() => new DbxRepository(dataPath));
        Assert.Contains("dataPath", exception.Message);
    }

    [TestMethod]
    public void Constructor_WithNull_Throws()
    {
        string dataPath = null!;
        var exception = Assert.ThrowsExactly<ArgumentNullException>(() => new DbxRepository(dataPath));
        Assert.Contains("dataPath", exception.Message);
    }

    [TestMethod]
    public async Task GetAllIdentifiers_WithMissingId_ReturnsEmpty()
    {
        var dataPath = "does-not-exist";
        var repository = new DbxRepository(dataPath);
        var identifiers = await repository.GetAllIdentifiersAsync(TestContext.CancellationToken);
        Assert.IsNotNull(identifiers);
        Assert.IsEmpty(identifiers);
    }

    [TestMethod]
    public async Task Create()
    {
        var dataPath = "test-folder";
        var id = "some-id";
        var itemId = "0000111122223333444455556666777788889999aaaabbbbccccddddeeeeffff";
        Directory.CreateDirectory(Path.Combine(dataPath, id));
        try
        {
            var item = JsonElement.Parse("{\"message\":\"this is a test\"}");
            var repository = new DbxRepository(dataPath);

            await repository.SaveItemAsync(id, itemId, item, TestContext.CancellationToken);
            var exists = await repository.ItemExistsAsync(id, itemId, TestContext.CancellationToken);
            var createdItem = await repository.GetItemAsync(id, itemId, TestContext.CancellationToken);

            Assert.IsTrue(exists);
            Assert.IsNotNull(createdItem);
            Assert.AreEqual(item.GetProperty("message").GetString(), ((JsonElement)createdItem).GetProperty("message").GetString());
        }
        finally
        {
            File.Delete(Path.Combine(dataPath, id, Path.ChangeExtension(itemId, "json")));
            Directory.Delete(Path.Combine(dataPath, id));
            Directory.Delete(dataPath);
        }

    }

    [TestMethod]
    public async Task GetAllItemIdentifiersAsync_ReturnsEmptyForMissingId()
    {
        var dataPath = "test-missing-id";
        var repository = new DbxRepository(dataPath);
        var result = await repository.GetAllItemIdentifiersAsync("0000111122223333444455556666777788889999aaaabbbbccccddddeeeeffff", TestContext.CancellationToken);
        Assert.IsNotNull(result);
        Assert.IsEmpty(result);
    }

    [TestMethod]
    public async Task SaveItemAsync_CreatesAndCanReadBack()
    {
        var dataPath = "test-save-read";
        var id = "0000111122223333444455556666777788889999aaaabbbbccccddddeeeeffff";
        var itemId = "111122223333444455556666777788889999aaaabbbbccccddddeeeeffff0000";
        Directory.CreateDirectory(Path.Combine(dataPath, id));
        try
        {
            var item = JsonElement.Parse("{\"foo\":42}");
            var repository = new DbxRepository(dataPath);
            await repository.SaveItemAsync(id, itemId, item, TestContext.CancellationToken);
            var read = await repository.GetItemAsync(id, itemId, TestContext.CancellationToken);
            Assert.IsNotNull(read);
            Assert.AreEqual(42, ((JsonElement)read).GetProperty("foo").GetInt32());
        }
        finally
        {
            File.Delete(Path.Combine(dataPath, id, Path.ChangeExtension(itemId, "json")));
            Directory.Delete(Path.Combine(dataPath, id));
            Directory.Delete(dataPath);
        }
    }

    [TestMethod]
    public async Task ItemExistsAsync_ReturnsFalseForMissing_TrueForPresent()
    {
        var dataPath = "test-exists";
        var id = "0000111122223333444455556666777788889999aaaabbbbccccddddeeeeffff";
        var itemId = "111122223333444455556666777788889999aaaabbbbccccddddeeeeffff0000";
        Directory.CreateDirectory(Path.Combine(dataPath, id));
        try
        {
            var repository = new DbxRepository(dataPath);
            Assert.IsFalse(await repository.ItemExistsAsync(id, itemId, TestContext.CancellationToken));
            var item = JsonElement.Parse("{\"bar\":123}");
            await repository.SaveItemAsync(id, itemId, item, TestContext.CancellationToken);
            Assert.IsTrue(await repository.ItemExistsAsync(id, itemId, TestContext.CancellationToken));
        }
        finally
        {
            File.Delete(Path.Combine(dataPath, id, Path.ChangeExtension(itemId, "json")));
            Directory.Delete(Path.Combine(dataPath, id));
            Directory.Delete(dataPath);
        }
    }

    [TestMethod]
    public async Task DeleteAsync_RemovesFile()
    {
        var dataPath = "test-delete";
        var id = "0000111122223333444455556666777788889999aaaabbbbccccddddeeeeffff";
        var itemId = "111122223333444455556666777788889999aaaabbbbccccddddeeeeffff0000";
        Directory.CreateDirectory(Path.Combine(dataPath, id));
        try
        {
            var repository = new DbxRepository(dataPath);
            var item = JsonElement.Parse("{\"baz\":true}");
            await repository.SaveItemAsync(id, itemId, item, TestContext.CancellationToken);
            Assert.IsTrue(await repository.ItemExistsAsync(id, itemId, TestContext.CancellationToken));
            await repository.DeleteAsync(id, itemId, TestContext.CancellationToken);
            Assert.IsFalse(await repository.ItemExistsAsync(id, itemId, TestContext.CancellationToken));
        }
        finally
        {
            File.Delete(Path.Combine(dataPath, id, Path.ChangeExtension(itemId, "json")));
            Directory.Delete(Path.Combine(dataPath, id));
            Directory.Delete(dataPath);
        }
    }

    [TestMethod]
    public async Task GetItemAsync_ReturnsNullForMissing()
    {
        var dataPath = "test-get-missing";
        var id = "0000111122223333444455556666777788889999aaaabbbbccccddddeeeeffff";
        var itemId = "111122223333444455556666777788889999aaaabbbbccccddddeeeeffff0000";
        var repository = new DbxRepository(dataPath);
        var result = await repository.GetItemAsync(id, itemId, TestContext.CancellationToken);
        Assert.IsNull(result);
    }

    [TestMethod]
    public async Task GetAllIdentifiersAsync_ReturnsAllIds()
    {
        var dataPath = "test-all-ids";
        var id1 = "0000111122223333444455556666777788889999aaaabbbbccccddddeeeeffff";
        var id2 = "111122223333444455556666777788889999aaaabbbbccccddddeeeeffff0000";
        var itemId = "22223333444455556666777788889999aaaabbbbccccddddeeeeffff00001111";
        Directory.CreateDirectory(Path.Combine(dataPath, id1));
        Directory.CreateDirectory(Path.Combine(dataPath, id2));
        try
        {
            var repository = new DbxRepository(dataPath);
            var item = JsonElement.Parse("{\"x\":1}");
            await repository.SaveItemAsync(id1, itemId, item, TestContext.CancellationToken);
            await repository.SaveItemAsync(id2, itemId, item, TestContext.CancellationToken);
            var ids = await repository.GetAllIdentifiersAsync(TestContext.CancellationToken);
            Assert.Contains(id1, ids);
            Assert.Contains(id2, ids);
        }
        finally
        {
            File.Delete(Path.Combine(dataPath, id1, Path.ChangeExtension(itemId, "json")));
            File.Delete(Path.Combine(dataPath, id2, Path.ChangeExtension(itemId, "json")));
            Directory.Delete(Path.Combine(dataPath, id1));
            Directory.Delete(Path.Combine(dataPath, id2));
            Directory.Delete(dataPath);
        }
    }


    [TestMethod]
    public async Task DeleteItemAsync_WithNullId_Throws()
    {
        var dataPath = "data=path";
        string id = null!;
        string itemId = null!;
        var repository = new DbxRepository(dataPath);
        await Assert.ThrowsExactlyAsync<FormatException>(() => repository.DeleteAsync(id, itemId, TestContext.CancellationToken));
    }

    [TestMethod]
    public async Task GetAllItemIdentifiersAsync_ReturnsAllItemIds()
    {
        var dataPath = "test-all-items";
        var id = "0000111122223333444455556666777788889999aaaabbbbccccddddeeeeffff";
        var itemId1 = "111122223333444455556666777788889999aaaabbbbccccddddeeeeffff0000";
        var itemId2 = "22223333444455556666777788889999aaaabbbbccccddddeeeeffff00001111";
        Directory.CreateDirectory(Path.Combine(dataPath, id));
        try
        {
            var repository = new DbxRepository(dataPath);
            var item = JsonElement.Parse("{\"y\":2}");
            await repository.SaveItemAsync(id, itemId1, item, TestContext.CancellationToken);
            await repository.SaveItemAsync(id, itemId2, item, TestContext.CancellationToken);
            var itemIds = await repository.GetAllItemIdentifiersAsync(id, TestContext.CancellationToken);
            Assert.Contains(itemId1, itemIds);
            Assert.Contains(itemId2, itemIds);
        }
        finally
        {
            File.Delete(Path.Combine(dataPath, id, Path.ChangeExtension(itemId1, "json")));
            File.Delete(Path.Combine(dataPath, id, Path.ChangeExtension(itemId2, "json")));
            Directory.Delete(Path.Combine(dataPath, id));
            Directory.Delete(dataPath);
        }
    }

    [TestMethod]
    public async Task SaveItemAsync_OverwritesExistingItem()
    {
        var dataPath = "test-overwrite";
        var id = "0000111122223333444455556666777788889999aaaabbbbccccddddeeeeffff";
        var itemId = "111122223333444455556666777788889999aaaabbbbccccddddeeeeffff0000";
        Directory.CreateDirectory(Path.Combine(dataPath, id));
        try
        {
            var repository = new DbxRepository(dataPath);
            var item1 = JsonElement.Parse("{\"z\":1}");
            var item2 = JsonElement.Parse("{\"z\":2}");
            await repository.SaveItemAsync(id, itemId, item1, TestContext.CancellationToken);
            await repository.SaveItemAsync(id, itemId, item2, TestContext.CancellationToken);
            var read = await repository.GetItemAsync(id, itemId, TestContext.CancellationToken);
            Assert.AreEqual(2, ((JsonElement)read).GetProperty("z").GetInt32());
        }
        finally
        {
            File.Delete(Path.Combine(dataPath, id, Path.ChangeExtension(itemId, "json")));
            Directory.Delete(Path.Combine(dataPath, id));
            Directory.Delete(dataPath);
        }
    }

    [TestMethod]
    public void GetAllItemIdentifiersAsync_ThrowsForInvalidId()
    {
        var dataPath = "test-check-id";
        var repository = new DbxRepository(dataPath);
        var id = @"~/\@#$";

        Assert.ThrowsExactly<FormatException>(() => repository.GetAllItemIdentifiersAsync(id, TestContext.CancellationToken));
    }

    [TestMethod]
    public void ItemExistsAsync_ThrowsForTooShortInvalidItemId()
    {
        var dataPath = "test-check-id";
        var id = "valid";
        var itemId = "abc"; // too small
        var repository = new DbxRepository(dataPath);

        Assert.ThrowsExactlyAsync<FormatException>(async () => await repository.ItemExistsAsync(id, itemId, TestContext.CancellationToken));
    }

    [TestMethod]
    public void ItemExistsAsync_ThrowsForInvalidItemId()
    {
        var dataPath = "test-check-id";
        var id = "valid";
        var itemId = "x11122223333444455556666777788889999aaaabbbbccccddddeeeeffff0000"; // 1 bad character
        var repository = new DbxRepository(dataPath);

        Assert.ThrowsExactlyAsync<FormatException>(async () => await repository.ItemExistsAsync(id, itemId, TestContext.CancellationToken));
    }

    [TestMethod]
    public void ItemExistsAsync_ThrowsForInvalidItemId2()
    {
        var dataPath = "test-check-id";
        var id = "valid";
        var itemId = "!11122223333444455556666777788889999aaaabbbbccccddddeeeeffff0000"; // 1 bad character
        var repository = new DbxRepository(dataPath);

        Assert.ThrowsExactlyAsync<FormatException>(async () => await repository.ItemExistsAsync(id, itemId, TestContext.CancellationToken));
    }

    [TestMethod]
    public void ItemExistsAsync_ThrowsForInvalidItemId3()
    {
        var dataPath = "test-check-id";
        var id = "valid";
        var itemId = "_11122223333444455556666777788889999aaaabbbbccccddddeeeeffff0000"; // 1 bad character
        var repository = new DbxRepository(dataPath);

        Assert.ThrowsExactlyAsync<FormatException>(async () => await repository.ItemExistsAsync(id, itemId, TestContext.CancellationToken));
    }

    [TestMethod]
    public void ItemExistsAsync_ThrowsForInvalidItemId4()
    {
        var dataPath = "test-check-id";
        var id = "valid";
        var itemId = "~11122223333444455556666777788889999aaaabbbbccccddddeeeeffff0000"; // 1 bad character
        var repository = new DbxRepository(dataPath);

        Assert.ThrowsExactlyAsync<FormatException>(async () => await repository.ItemExistsAsync(id, itemId, TestContext.CancellationToken));
    }




    public TestContext TestContext { get; set; }    // MSTest will set this value
}
