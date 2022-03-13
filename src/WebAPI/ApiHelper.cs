using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Zs.Common.Extensions;

namespace Zs.Common.Services.WebAPI
{
    public static class ApiHelper
    {
        private static readonly HttpClient _httpClient = new HttpClient();

        public static async Task<TResult> GetAsync<TResult>(
            string requestUri,
            string mediaType = null,
            string userAgent = null,
            bool throwExceptionOnError = true)
        {
            try
            {
                PrepareClient(mediaType, userAgent);
                return await _httpClient.GetAsync<TResult>(requestUri).ConfigureAwait(false);
            }
            catch(Exception ex)
            {
                if (throwExceptionOnError)
                {
                    ex.Data.Add("URI", requestUri);
                    ex.Data.Add("MediaType", mediaType);
                    ex.Data.Add("UserAgent", userAgent);

                    throw ex;
                }

                return default(TResult);
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
                return await _httpClient.GetStringAsync(requestUri).ConfigureAwait(false);
            }
            catch(Exception ex)
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
            _httpClient.DefaultRequestHeaders.Accept.Clear();

            if (mediaType != null)
            {
                _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(mediaType));
            }

            if (userAgent != null)
            {
                _httpClient.DefaultRequestHeaders.Add("User-Agent", userAgent);
            }
        }
    }
}
