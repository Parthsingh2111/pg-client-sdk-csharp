using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Utils; // Assuming namespace for Logger

namespace Helpers
{
    /// <summary>
    /// Provides methods for making HTTP requests with timeout protection.
    /// </summary>
    public static class HttpClientHelper
    {
        private static readonly HttpClient Client = new HttpClient();
        private const string SdkVersion = "PayGlocal-SDK/1.0.3"; // Hardcoded version from JavaScript comment

        /// <summary>
        /// Makes an HTTP request with timeout protection.
        /// </summary>
        /// <param name="url">The request URL.</param>
        /// <param name="options">The HTTP request options.</param>
        /// <returns>The response data deserialized as an object.</returns>
        /// <exception cref="HttpRequestException">Thrown when the request fails.</exception>
        private static async Task<object> MakeRequest(string url, HttpRequestOptions options)
        {
            using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(90))) // 90-second timeout
            {
                try
                {
                    var request = new HttpRequestMessage
                    {
                        Method = new HttpMethod(options.Method),
                        RequestUri = new Uri(url)
                    };

                    // Add headers, including SDK version
                    request.Headers.Add("pg-sdk-version", SdkVersion);
                    if (options.Headers != null)
                    {
                        foreach (var header in options.Headers)
                        {
                            request.Headers.TryAddWithoutValidation(header.Key, header.Value);
                        }
                    }

                    // Add content if provided
                    if (options.Body != null)
                    {
                        request.Content = options.Body;
                    }

                    var response = await Client.SendAsync(request, cts.Token);
                    response.EnsureSuccessStatusCode();

                    var content = await response.Content.ReadAsStringAsync();
                    return string.IsNullOrEmpty(content) ? new object() : JsonSerializer.Deserialize<object>(content);
                }
                catch (OperationCanceledException)
                {
                    throw new HttpRequestException("Request timed out after 90 seconds");
                }
                catch (HttpRequestException ex)
                {
                    throw new HttpRequestException($"HTTP request failed: {ex.Message}", ex);
                }
            }
        }

        /// <summary>
        /// Makes a POST request.
        /// </summary>
        /// <param name="url">The request URL.</param>
        /// <param name="data">The request payload (object or string).</param>
        /// <param name="headers">Optional request headers.</param>
        /// <returns>The response data.</returns>
        /// <exception cref="HttpRequestException">Thrown when the request fails.</exception>
        public static async Task<object> Post(string url, object data, Dictionary<string, string> headers = null)
        {
            try
            {
                var logger = new Logger();
                logger.LogRequest("POST", url, headers, data);

                HttpContent content = null;
                var optionsHeaders = headers ?? new Dictionary<string, string>();

                if (data is string stringData)
                {
                    content = new StringContent(stringData, Encoding.UTF8, "text/plain");
                    optionsHeaders["Content-Type"] = "text/plain";
                }
                else if (data != null)
                {
                    string jsonData = JsonSerializer.Serialize(data);
                    content = new StringContent(jsonData, Encoding.UTF8, "application/json");
                    optionsHeaders["Content-Type"] = "application/json";
                }

                var options = new HttpRequestOptions
                {
                    Method = "POST",
                    Headers = optionsHeaders,
                    Body = content
                };

                var response = await MakeRequest(url, options);
                var logger = new Logger();
                logger.LogResponse("POST", url, 200, response);

                return response ?? new object();
            }
            catch (Exception ex)
            {
                Logger.Error($"POST request failed: {url}", ex);
                throw;
            }
        }

        /// <summary>
        /// Makes a GET request.
        /// </summary>
        /// <param name="url">The request URL.</param>
        /// <param name="headers">Optional request headers.</param>
        /// <returns>The response data.</returns>
        /// <exception cref="HttpRequestException">Thrown when the request fails.</exception>
        public static async Task<object> Get(string url, Dictionary<string, string> headers = null)
        {
            try
            {
                var logger = new Logger();
                logger.LogRequest("GET", url, headers);

                var options = new HttpRequestOptions
                {
                    Method = "GET",
                    Headers = headers ?? new Dictionary<string, string>()
                };

                var response = await MakeRequest(url, options);
                var logger = new Logger();
                logger.LogResponse("GET", url, 200, response);

                return response ?? new object();
            }
            catch (Exception ex)
            {
                Logger.Error($"GET request failed: {url}", ex);
                throw;
            }
        }

        /// <summary>
        /// Makes a PUT request.
        /// </summary>
        /// <param name="url">The request URL.</param>
        /// <param name="data">The request payload (object or string).</param>
        /// <param name="headers">Optional request headers.</param>
        /// <returns>The response data.</returns>
        /// <exception cref="HttpRequestException">Thrown when the request fails.</exception>
        public static async Task<object> Put(string url, object data, Dictionary<string, string> headers = null)
        {
            try
            {
                var logger = new Logger();
                logger.LogRequest("PUT", url, headers, data);

                HttpContent content = null;
                var optionsHeaders = headers ?? new Dictionary<string, string>();

                if (data is string stringData)
                {
                    content = new StringContent(stringData, Encoding.UTF8, "text/plain");
                    optionsHeaders["Content-Type"] = "text/plain";
                }
                else if (data != null)
                {
                    string jsonData = JsonSerializer.Serialize(data);
                    content = new StringContent(jsonData, Encoding.UTF8, "application/json");
                    optionsHeaders["Content-Type"] = "application/json";
                }

                var options = new HttpRequestOptions
                {
                    Method = "PUT",
                    Headers = optionsHeaders,
                    Body = content
                };

                var response = await MakeRequest(url, options);
                var logger = new Logger();
                logger.LogResponse("PUT", url, 200, response);

                return response ?? new object();
            }
            catch (Exception ex)
            {
                Logger.Error($"PUT request failed: {url}", ex);
                throw;
            }
        }
    }

    /// <summary>
    /// Represents options for an HTTP request.
    /// </summary>
    public class HttpRequestOptions
    {
        public string Method { get; set; }
        public Dictionary<string, string> Headers { get; set; }
        public HttpContent Body { get; set; }
    }
}