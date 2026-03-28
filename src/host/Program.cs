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

        builder.Services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                policy.WithOrigins(
                    "http://localhost", "https://localhost",
                    "http://127.0.0.1", "https://127.0.0.1")
                .SetIsOriginAllowed(origin =>
                    origin.StartsWith("http://localhost") ||
                    origin.StartsWith("https://localhost") ||
                    origin.StartsWith("http://127.0.0.1") ||
                    origin.StartsWith("https://127.0.0.1"))
                .AllowAnyHeader()
                .AllowAnyMethod();
            });
        });

        // Configure the HTTP request pipeline.
        var app = builder.Build();
        app.UseCors();
        app.UseDbx();
        app.Run();
    }
}
