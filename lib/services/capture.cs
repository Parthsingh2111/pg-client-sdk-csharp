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
    /// Provides methods to initiate capture operations.
    /// </summary>
    public static class CaptureService
    {
        /// <summary>
        /// Initiates a capture for an authorized transaction.
        /// </summary>
        /// <param name="parameters">Capture parameters including gid and capture details.</param>
        /// <param name="config">Configuration object.</param>
        /// <returns>Task representing the capture response.</returns>
        public static async Task<object> InitiateCapture(object parameters, Config config)
        {
            var logger = new Logger(config.LogLevel);
            logger.Info("Initiating capture", new
            {
                Gid = GetProperty(parameters, "gid")
            });

            // Validate required fields
            string[] requiredFields = { "gid", "captureAmount" };
            PayloadValidationHelper.ValidatePayload(parameters, new PayloadValidationOptions
            {
                RequiredFields = requiredFields,
                ValidateSchema = false
            });

            // Generate tokens for JWT authentication
            var tokens = await TokenHelper.GenerateTokens(parameters, config, "capture");
            var headers = HeaderHelper.BuildJwtHeaders(tokens.Jws);

            // Make API request
            string gid = GetProperty(parameters, "gid")?.ToString() ?? "";
            var response = await ApiRequestHelper.MakeTransactionServiceRequest(new ApiRequestOptions
            {
                Method = "POST",
                BaseUrl = config.BaseUrl,
                Endpoint = Endpoints.TransactionService.Capture,
                Gid = gid,
                RequestData = tokens.Jwe,
                Headers = headers,
                Operation = "capture"
            });

            logger.Info("Capture completed successfully", new { Gid = gid });

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
