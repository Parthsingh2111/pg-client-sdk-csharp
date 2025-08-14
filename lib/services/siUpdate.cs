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
    /// Provides methods to manage Standing Instruction operations.
    /// </summary>
    public static class SiUpdateService
    {
        /// <summary>
        /// Initiates a pause operation for a Standing Instruction.
        /// </summary>
        /// <param name="parameters">SI pause parameters including siId.</param>
        /// <param name="config">Configuration object.</param>
        /// <returns>Task representing the pause response.</returns>
        public static async Task<object> InitiatePauseSI(object parameters, Config config)
        {
            var logger = new Logger(null, config.LogLevel);
            logger.Info("Initiating SI pause", new
            {
                SiId = GetProperty(parameters, "siId")
            });

            // Validate required fields
            string[] requiredFields = { "siId", "action" };
            PayloadValidationHelper.ValidatePayload(parameters, new PayloadValidationOptions
            {
                RequiredFields = requiredFields,
                ValidateSchema = false
            });

            // Ensure action is set to "pause"
            var actionValue = GetProperty(parameters, "action")?.ToString();
            if (actionValue != "pause")
            {
                throw new ArgumentException("Action must be 'pause' for pause SI operation");
            }

            // Generate tokens for JWT authentication
            var tokens = await TokenHelper.GenerateTokens(parameters, config, "si-pause");
            var headers = HeaderHelper.BuildSiHeaders(tokens.Jws);

            // Make API request
            var response = await ApiRequestHelper.MakeSiServiceRequest(new RequestOptions
            {
                Method = "POST",
                BaseUrl = config.BaseUrl,
                Endpoint = Endpoints.SiService.Modify,
                RequestData = tokens.Jwe,
                Headers = headers,
                Operation = "si-pause"
            });

            logger.Info("SI pause completed successfully", new { SiId = GetProperty(parameters, "siId") });

            return response;
        }

        /// <summary>
        /// Initiates an activation operation for a Standing Instruction.
        /// </summary>
        /// <param name="parameters">SI activation parameters including siId.</param>
        /// <param name="config">Configuration object.</param>
        /// <returns>Task representing the activation response.</returns>
        public static async Task<object> InitiateActivateSI(object parameters, Config config)
        {
            var logger = new Logger(null, config.LogLevel);
            logger.Info("Initiating SI activation", new
            {
                SiId = GetProperty(parameters, "siId")
            });

            // Validate required fields
            string[] requiredFields = { "siId", "action" };
            PayloadValidationHelper.ValidatePayload(parameters, new PayloadValidationOptions
            {
                RequiredFields = requiredFields,
                ValidateSchema = false
            });

            // Ensure action is set to "activate"
            var actionValue = GetProperty(parameters, "action")?.ToString();
            if (actionValue != "activate")
            {
                throw new ArgumentException("Action must be 'activate' for activate SI operation");
            }

            // Generate tokens for JWT authentication
            var tokens = await TokenHelper.GenerateTokens(parameters, config, "si-activate");
            var headers = HeaderHelper.BuildSiHeaders(tokens.Jws);

            // Make API request
            var response = await ApiRequestHelper.MakeSiServiceRequest(new RequestOptions
            {
                Method = "POST",
                BaseUrl = config.BaseUrl,
                Endpoint = Endpoints.SiService.Modify,
                RequestData = tokens.Jwe,
                Headers = headers,
                Operation = "si-activate"
            });

            logger.Info("SI activation completed successfully", new { SiId = GetProperty(parameters, "siId") });

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
