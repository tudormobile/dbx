using System.Diagnostics.CodeAnalysis;
namespace Dbx.Tests
{
    [ExcludeFromCodeCoverage]
    public class MockHttpMessageHandler : HttpMessageHandler
    {
        public Uri? ProvidedRequestUri { get; set; }
        public Exception? AlwaysThrows { get; set; } = null;
        public HttpResponseMessage? AlwaysResponds { get; set; } = null;
        public string? JsonResponse { get; set; }
        public string? JsonSecondaryResponse { get; set; }
        public bool ForceNullResponse { get; set; } = false;
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            ProvidedRequestUri = request.RequestUri;
            return await Task.Run(() =>
            {
                if (AlwaysThrows != null)
                {
                    throw AlwaysThrows;
                }
                if (AlwaysResponds != null)
                {
                    if (JsonResponse != null)
                    {
                        var json = JsonResponse;
                        JsonResponse = JsonSecondaryResponse ?? JsonResponse;
                        JsonSecondaryResponse = null;
                        AlwaysResponds.Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                    }
                    return AlwaysResponds;
                }
                if (JsonResponse != null)
                {
                    var json = JsonResponse;
                    JsonResponse = JsonSecondaryResponse ?? JsonResponse;
                    JsonSecondaryResponse = null;
                    return new HttpResponseMessage(System.Net.HttpStatusCode.OK)
                    {
                        Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json")
                    };
                }
                if (ForceNullResponse)
                {
                    return null!;
                }
                return new HttpResponseMessage(System.Net.HttpStatusCode.InternalServerError);
            });
        }
    }
}
