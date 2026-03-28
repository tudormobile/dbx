using System.Text.Json;

class Program
{
    static async Task Main()
    {
        Console.WriteLine("Hello, World!");
        using var client = new HttpClient();
        var response = await client.GetAsync("http://localhost:5284/api/v1/dbx/status");
        Console.WriteLine($"Response: {response.StatusCode}");
        var content = await response.Content.ReadAsStringAsync();
        Console.WriteLine(content);

        var responseObject = JsonSerializer.Deserialize<ResponseMessage>(
            content,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
           );

        Console.WriteLine($"Success: {responseObject!.Success}");
        Console.WriteLine($"Data   : {responseObject!.Data}");
        Console.WriteLine($"Version: {responseObject!.Data!.Version}");
    }

    class ResponseMessage
    {
        public bool Success { get; set; }
        public ServiceVersion? Data { get; set; }
    }

    class ServiceVersion
    {
        public string? Version { get; set; }
        public override string ToString() => $"(Version = {Version})";
    }

}