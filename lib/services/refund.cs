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
    /// Provides methods to initiate refund operations.
    /// </summary>
    public static class RefundService
    {
        /// <summary>
        /// Initiates a refund for a transaction.
        /// </summary>
        /// <param name="parameters">Refund parameters including gid and refund details.</param>
        /// <param name="config">Configuration object.</param>
        /// <returns>Task representing the refund response.</returns>
        public static async Task<object> InitiateRefund(object parameters, Config config)
        {
            var logger = new Logger(null, config.LogLevel);
            logger.Info("Initiating refund", new
            {
                Gid = GetProperty(parameters, "gid")
            });

            // Validate required fields
            string[] requiredFields = { "gid", "refundAmount" };
            PayloadValidationHelper.ValidatePayload(parameters, new PayloadValidationOptions
            {
                RequiredFields = requiredFields,
                ValidateSchema = false
            });

            // Generate tokens for JWT authentication
            var tokens = await TokenHelper.GenerateTokens(parameters, config, "refund");
            var headers = HeaderHelper.BuildJwtHeaders(tokens.Jws);

            // Make API request
            string gid = GetProperty(parameters, "gid")?.ToString();
            var response = await ApiRequestHelper.MakeTransactionServiceRequest(new RequestOptions
            {
                Method = "POST",
                BaseUrl = config.BaseUrl,
                Endpoint = Endpoints.TransactionService.Refund,
                Gid = gid,
                RequestData = tokens.Jwe,
                Headers = headers,
                Operation = "refund"
            });

            logger.Info("Refund completed successfully", new
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
