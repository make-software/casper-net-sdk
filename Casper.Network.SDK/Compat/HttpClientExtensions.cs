#if NETSTANDARD2_0

using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

public static class HttpClientExtensions
{
    public static async Task<Stream> GetStreamAsync(this HttpClient client, Uri uri, CancellationToken cancelToken)
    {
        var response = await client.GetAsync(uri, cancelToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"Request to {uri} returned with HTTP status code {response.StatusCode}");
        }
        return await response.Content.ReadAsStreamAsync();
    }
}

#endif
