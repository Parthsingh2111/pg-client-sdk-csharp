using System;
using System.Collections.Generic;

namespace Helpers
{
    /// <summary>
    /// Provides methods for building HTTP headers for PayGlocal API requests.
    /// </summary>
    public static class HeaderHelper
    {
        /// <summary>
        /// Builds headers for JWT-based requests.
        /// </summary>
        /// <param name="jws">The JWS token.</param>
        /// <param name="customHeaders">Additional custom headers.</param>
        /// <returns>A dictionary of headers.</returns>
        public static Dictionary<string, string> BuildJwtHeaders(string jws, Dictionary<string, string> customHeaders = null)
        {
            if (string.IsNullOrEmpty(jws))
            {
                throw new ArgumentException("JWS token cannot be null or empty", nameof(jws));
            }

            var headers = new Dictionary<string, string>
            {
                { "Content-Type", "text/plain" },
                { "x-gl-token-external", jws }
            };

            if (customHeaders != null)
            {
                foreach (var header in customHeaders)
                {
                    headers[header.Key] = header.Value;
                }
            }

            return headers;
        }

        /// <summary>
        /// Builds headers for API key-based requests.
        /// </summary>
        /// <param name="apiKey">The API key.</param>
        /// <param name="customHeaders">Additional custom headers.</param>
        /// <returns>A dictionary of headers.</returns>
        public static Dictionary<string, string> BuildApiKeyHeaders(string apiKey, Dictionary<string, string> customHeaders = null)
        {
            if (string.IsNullOrEmpty(apiKey))
            {
                throw new ArgumentException("API key cannot be null or empty", nameof(apiKey));
            }

            var headers = new Dictionary<string, string>
            {
                { "Content-Type", "application/json" },
                { "x-gl-auth", apiKey }
            };

            if (customHeaders != null)
            {
                foreach (var header in customHeaders)
                {
                    headers[header.Key] = header.Value;
                }
            }

            return headers;
        }

        /// <summary>
        /// Builds headers for Standing Instruction (SI) operations.
        /// </summary>
        /// <param name="jws">The JWS token.</param>
        /// <returns>A dictionary of headers.</returns>
        public static Dictionary<string, string> BuildSiHeaders(string jws)
        {
            if (string.IsNullOrEmpty(jws))
            {
                throw new ArgumentException("JWS token cannot be null or empty", nameof(jws));
            }

            return new Dictionary<string, string>
            {
                { "Content-Type", "text/plain" },
                { "x-gl-token-external", jws }
            };
        }
    }
}