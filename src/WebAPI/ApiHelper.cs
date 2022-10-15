using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Zs.Common.Extensions;

namespace Zs.Common.Services.WebAPI;

public static class ApiHelper
{
    private static readonly HttpClient HttpClient = new();

    public static async Task<TResult> GetAsync<TResult>(
        string requestUri,
        string mediaType = null,
        string userAgent = null,
        bool throwExceptionOnError = true)
    {
        try
        {
            PrepareClient(mediaType, userAgent);
            return await HttpClient.GetAsync<TResult>(requestUri).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            if (throwExceptionOnError)
            {
                ex.Data.Add("URI", requestUri);
                ex.Data.Add("MediaType", mediaType);
                ex.Data.Add("UserAgent", userAgent);

                throw ex;
            }

            return default;
        }
    }

    public static async Task<string> GetAsync(
        string requestUri,
        string mediaType = null,
        string userAgent = null,
        bool throwExceptionOnError = true)
    {
        try
        {
            PrepareClient(mediaType, userAgent);
            return await HttpClient.GetStringAsync(requestUri).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            if (throwExceptionOnError)
            {
                ex.Data.Add("URI", requestUri);
                ex.Data.Add("MediaType", mediaType);
                ex.Data.Add("UserAgent", userAgent);

                throw ex;
            }

            return null;
        }
    }

    private static void PrepareClient(
        string mediaType = null,
        string userAgent = null)
    {
        HttpClient.DefaultRequestHeaders.Accept.Clear();

        if (mediaType != null)
        {
            HttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(mediaType));
        }

        if (userAgent != null)
        {
            HttpClient.DefaultRequestHeaders.Add("User-Agent", userAgent);
        }
    }
}