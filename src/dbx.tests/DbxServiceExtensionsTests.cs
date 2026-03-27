using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Dbx.Tests;

[TestClass]
public class DbxServiceExtensionsTests
{
    [TestMethod]
    public void AddDbx_ReturnsServiceCollection()
    {
        var services = new ServiceCollection();

        var result = services.AddDbx();

        Assert.AreSame(services, result);
    }

    [TestMethod]
    public void AddDbx_NullServices_Throws()
        => Assert.ThrowsExactly<ArgumentNullException>(() =>
            ((IServiceCollection)null!).AddDbx());

    [TestMethod]
    public void AddDbx_RegistersIDbxService()
    {
        var services = new ServiceCollection();
        services.AddDbx();

        var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IDbxService));

        Assert.IsNotNull(descriptor);
    }

    [TestMethod]
    public void AddDbx_IDbxServiceIsScoped()
    {
        var services = new ServiceCollection();
        services.AddDbx();

        var descriptor = services.Single(d => d.ServiceType == typeof(IDbxService));

        Assert.AreEqual(ServiceLifetime.Scoped, descriptor.Lifetime);
    }

    [TestMethod]
    public void UseDbx_ReturnsApp()
    {
        var app = BuildApp();

        var result = app.UseDbx();

        Assert.AreSame(app, result);
    }


    private static WebApplication BuildApp()
    {
        var builder = WebApplication.CreateSlimBuilder();
        builder.Logging.ClearProviders();
        builder.Services.AddDbx();
        return builder.Build();
    }
}
