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
    /// Provides methods to initiate authorization reversal operations.
    /// </summary>
    public static class ReversalService
    {
        /// <summary>
        /// Initiates an authorization reversal for a transaction.
        /// </summary>
        /// <param name="parameters">Reversal parameters including gid and reversal details.</param>
        /// <param name="config">Configuration object.</param>
        /// <returns>Task representing the reversal response.</returns>
        public static async Task<object> InitiateAuthReversal(object parameters, Config config)
        {
            var logger = new Logger(config.LogLevel);
            logger.Info("Initiating auth reversal", new
            {
                Gid = GetProperty(parameters, "gid")
            });

            // Validate required fields
            string[] requiredFields = { "gid", "reversalAmount" };
            PayloadValidationHelper.ValidatePayload(parameters, new PayloadValidationOptions
            {
                RequiredFields = requiredFields,
                ValidateSchema = false
            });

            // Generate tokens for JWT authentication
            var tokens = await TokenHelper.GenerateTokens(parameters, config, "auth-reversal");
            var headers = HeaderHelper.BuildJwtHeaders(tokens.Jws);

            // Make API request
            string gid = GetProperty(parameters, "gid")?.ToString() ?? "";
            var response = await ApiRequestHelper.MakeTransactionServiceRequest(new ApiRequestOptions
            {
                Method = "POST",
                BaseUrl = config.BaseUrl,
                Endpoint = Endpoints.TransactionService.AuthReversal,
                Gid = gid,
                RequestData = tokens.Jwe,
                Headers = headers,
                Operation = "auth-reversal"
            });

            logger.Info("Auth reversal completed successfully", new { Gid = gid });

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
