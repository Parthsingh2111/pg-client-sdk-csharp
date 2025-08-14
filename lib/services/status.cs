using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Helpers;
using Constants;
using Utils;
using Core;

namespace Services
{
    /// <summary>
    /// Provides methods to check transaction status.
    /// </summary>
    public static class StatusService
    {
        /// <summary>
        /// Initiates a status check for a transaction.
        /// </summary>
        /// <param name="parameters">Status check parameters including gid.</param>
        /// <param name="config">Configuration object.</param>
        /// <returns>Task representing the status response.</returns>
        public static async Task<object> InitiateCheckStatus(object parameters, Config config)
        {
            var logger = new Logger(config.LogLevel);
            logger.Info("Initiating status check", new
            {
                Gid = GetProperty(parameters, "gid")
            });

            // Validate required fields
            string[] requiredFields = { "gid" };
            PayloadValidationHelper.ValidatePayload(parameters, new PayloadValidationOptions
            {
                RequiredFields = requiredFields,
                ValidateSchema = false
            });

            // Build digest input as the full endpoint path for GET
            string gid = GetProperty(parameters, "gid")?.ToString();
            string digestPath = Endpoints.BuildEndpoint(Endpoints.TransactionService.Status, new Dictionary<string, string> { { "gid", gid } });

            // Generate tokens for JWT authentication (JWS signs the request path for GET)
            var tokens = await TokenHelper.GenerateTokens(parameters, config, "status", digestPath);
            var headers = HeaderHelper.BuildJwtHeaders(tokens.Jws);

            // Make API request
            var response = await ApiRequestHelper.MakeTransactionServiceRequest(new RequestOptions
            {
                Method = "GET",
                BaseUrl = config.BaseUrl,
                Endpoint = Endpoints.TransactionService.Status,
                Gid = gid,
                RequestData = null,
                Headers = headers,
                Operation = "status"
            });

            logger.Info("Status check completed successfully", new { Gid = gid });

            return response;
        }

        // Helper method to safely access dynamic properties
        private static object GetProperty(object obj, string propertyName)
        {
            if (obj == null) return null;
            var prop = obj.GetType().GetProperty(propertyName);
            return prop?.GetValue(obj);
        }
    }
}
