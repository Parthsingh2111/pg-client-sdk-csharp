using System;
using System.Collections.Generic;

namespace Constants
{
    /// <summary>
    /// Defines PayGlocal API endpoints and utility methods for building endpoint URLs.
    /// </summary>
    public static class Endpoints
    {
        /// <summary>
        /// Payment-related endpoints.
        /// </summary>
        public static class Payment
        {
            /// <summary>
            /// Endpoint for initiating a payment.
            /// </summary>
            public const string Initiate = "/gl/v1/payments/initiate/paycollect";
        }

        /// <summary>
        /// Transaction-related endpoints.
        /// </summary>
        public static class TransactionService
        {
            /// <summary>
            /// Endpoint for checking transaction status. Requires {gid} placeholder.
            /// </summary>
            public const string Status = "/gl/v1/payments/{gid}/status";

            /// <summary>
            /// Endpoint for initiating a refund. Requires {gid} placeholder.
            /// </summary>
            public const string Refund = "/gl/v1/payments/{gid}/refund";

            /// <summary>
            /// Endpoint for capturing a payment. Requires {gid} placeholder.
            /// </summary>
            public const string Capture = "/gl/v1/payments/{gid}/capture";

            /// <summary>
            /// Endpoint for reversing an authorization. Requires {gid} placeholder.
            /// </summary>
            public const string AuthReversal = "/gl/v1/payments/{gid}/auth-reversal";
        }

        /// <summary>
        /// Standing Instruction (SI) related endpoints.
        /// </summary>
        public static class SiService
        {
            /// <summary>
            /// Endpoint for modifying a Standing Instruction.
            /// </summary>
            public const string Modify = "/gl/v1/payments/si/modify";

            /// <summary>
            /// Endpoint for checking SI status.
            /// </summary>
            public const string Status = "/gl/v1/payments/si/status";
        }

        /// <summary>
        /// Builds an endpoint URL by replacing placeholders with parameter values.
        /// </summary>
        /// <param name="endpoint">The base endpoint URL with placeholders (e.g., {gid}).</param>
        /// <param name="parameters">Dictionary of parameter key-value pairs to replace placeholders.</param>
        /// <returns>The complete endpoint URL with placeholders replaced.</returns>
        public static string BuildEndpoint(string endpoint, Dictionary<string, string> parameters = null)
        {
            if (string.IsNullOrEmpty(endpoint))
            {
                throw new ArgumentException("Endpoint cannot be null or empty.", nameof(endpoint));
            }

            string url = endpoint;
            if (parameters != null)
            {
                foreach (var param in parameters)
                {
                    url = url.Replace($"{{{param.Key}}}", param.Value);
                }
            }

            return url;
        }
    }
}