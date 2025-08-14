using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Constants; // Assuming namespace for Endpoints
using Utils; // Assuming namespace for Logger

namespace Helpers
{
    /// <summary>
    /// Provides methods for making API requests with simple error handling.
    /// </summary>
    public static class ApiRequestHelper
    {
        /// <summary>
        /// Makes an API request with the specified options.
        /// </summary>
        /// <param name="options">The request options.</param>
        /// <returns>The API response.</returns>
        /// <exception cref="ArgumentException">Thrown when required options are missing.</exception>
        /// <exception cref="HttpRequestException">Thrown when the API request fails.</exception>
        public static async Task<object> MakeApiRequest(RequestOptions options)
        {
            try
            {
                if (string.IsNullOrEmpty(options.BaseUrl))
                    throw new ArgumentException("Base URL is required");
                if (string.IsNullOrEmpty(options.Endpoint))
                    throw new ArgumentException("Endpoint is required");

                // Build full URL
                string fullEndpoint = Endpoints.BuildEndpoint(options.Endpoint, options.EndpointParams);
                string fullUrl = $"{options.BaseUrl}{fullEndpoint}";

                // Make the API call
                object response;
                string method = options.Method?.ToUpper() ?? "POST";
                switch (method)
                {
                    case "GET":
                        response = await HttpClientHelper.Get(fullUrl, options.Headers);
                        break;
                    case "PUT":
                        response = await HttpClientHelper.Put(fullUrl, options.RequestData, options.Headers);
                        break;
                    default:
                        response = await HttpClientHelper.Post(fullUrl, options.RequestData, options.Headers);
                        break;
                }

                // Validate response exists
                if (response == null)
                {
                    throw new HttpRequestException("Empty response from API");
                }

                return response;
            }
            catch (Exception ex)
            {
                Logger.Error($"API request failed: {options.Endpoint}", ex);
                throw;
            }
        }

        /// <summary>
        /// Makes a payment request.
        /// </summary>
        /// <param name="options">The request options.</param>
        /// <returns>The API response.</returns>
        public static async Task<object> MakePaymentRequest(RequestOptions options)
        {
            return await MakeApiRequest(new RequestOptions
            {
                Method = options.Method,
                BaseUrl = options.BaseUrl,
                Endpoint = options.Endpoint,
                RequestData = options.RequestData,
                Headers = options.Headers
            });
        }

        /// <summary>
        /// Makes a transaction service request.
        /// </summary>
        /// <param name="options">The request options.</param>
        /// <returns>The API response.</returns>
        public static async Task<object> MakeTransactionServiceRequest(RequestOptions options)
        {
            return await MakeApiRequest(new RequestOptions
            {
                Method = options.Method,
                BaseUrl = options.BaseUrl,
                Endpoint = options.Endpoint,
                EndpointParams = new Dictionary<string, string> { { "gid", options.Gid } },
                RequestData = options.RequestData,
                Headers = options.Headers
            });
        }

        /// <summary>
        /// Makes a Standing Instruction (SI) service request.
        /// </summary>
        /// <param name="options">The request options.</param>
        /// <returns>The API response.</returns>
        public static async Task<object> MakeSiServiceRequest(RequestOptions options)
        {
            return await MakeApiRequest(new RequestOptions
            {
                Method = options.Method,
                BaseUrl = options.BaseUrl,
                Endpoint = options.Endpoint,
                RequestData = options.RequestData,
                Headers = options.Headers
            });
        }
    }

    /// <summary>
    /// Represents options for an API request.
    /// </summary>
    public class RequestOptions
    {
        public string Method { get; set; } = "POST";
        public string BaseUrl { get; set; }
        public string Endpoint { get; set; }
        public Dictionary<string, string> EndpointParams { get; set; }
        public object RequestData { get; set; }
        public Dictionary<string, string> Headers { get; set; }
        public string Operation { get; set; }
        public string Gid { get; set; }
    }
}