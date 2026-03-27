using Tudormobile.Dbx;

namespace dbx;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddDbx(options =>
        {
            options.Prefix = "api/v1";  // endpoint mapping prefix
            options.DataPath = "data";  // repository location
        });

        // Configure the HTTP request pipeline.
        var app = builder.Build();
        app.UseDbx();
        app.Run();
    }
}
