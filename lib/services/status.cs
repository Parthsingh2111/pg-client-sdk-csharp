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
            var logger = new Logger(null, config.LogLevel);
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

            // Generate tokens for JWT authentication
            var tokens = await TokenHelper.GenerateTokens(parameters, config, "status");
            var headers = HeaderHelper.BuildJwtHeaders(tokens.Jws);

            // Make API request
            string gid = GetProperty(parameters, "gid")?.ToString();
            var response = await ApiRequestHelper.MakeTransactionServiceRequest(new RequestOptions
            {
                Method = "GET",
                BaseUrl = config.BaseUrl,
                Endpoint = Endpoints.TransactionService.Status,
                Gid = gid,
                RequestData = null, // GET request doesn't need body
                Headers = headers,
                Operation = "status"
            });

            logger.Info("Status check completed successfully", new
            {
                Gid = gid,
                ResponseStatus = GetProperty(response, "status") ?? "unknown"
            });

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
