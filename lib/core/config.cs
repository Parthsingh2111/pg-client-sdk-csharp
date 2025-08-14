using System;
using System.Collections.Generic;
using Utils; // Assuming namespace for Logger

namespace Core
{
    /// <summary>
    /// Configuration class for PayGlocalClient.
    /// </summary>
    public class Config
    {
        private static readonly Dictionary<string, string> BaseUrls = new Dictionary<string, string>
        {
            { "UAT", "https://api.uat.payglocal.in" },
            { "PROD", "https://api.payglocal.in" }
        };

        /// <summary>
        /// Gets the API key for authentication.
        /// </summary>
        public string ApiKey { get; }

        /// <summary>
        /// Gets the merchant ID.
        /// </summary>
        public string MerchantId { get; }

        /// <summary>
        /// Gets the public key ID for token-based authentication.
        /// </summary>
        public string PublicKeyId { get; }

        /// <summary>
        /// Gets the private key ID for token-based authentication.
        /// </summary>
        public string PrivateKeyId { get; }

        /// <summary>
        /// Gets the PayGlocal public key for token-based authentication.
        /// </summary>
        public string PayglocalPublicKey { get; }

        /// <summary>
        /// Gets the merchant private key for token-based authentication.
        /// </summary>
        public string MerchantPrivateKey { get; }

        /// <summary>
        /// Gets the PayGlocal environment (e.g., UAT, PROD).
        /// </summary>
        public string PayglocalEnv { get; }

        /// <summary>
        /// Gets the base URL based on the Payglocal environment.
        /// </summary>
        public string BaseUrl { get; }

        /// <summary>
        /// Gets the logging level.
        /// </summary>
        public string LogLevel { get; }

        /// <summary>
        /// Gets the token expiration time in milliseconds.
        /// </summary>
        public int TokenExpiration { get; }

        /// <summary>
        /// Initializes a new instance of the Config class.
        /// </summary>
        /// <param name="config">Configuration object with optional properties.</param>
        /// <exception cref="ArgumentException">Thrown when required configuration is missing or invalid.</exception>
        public Config(dynamic config = null)
        {
            try
            {
                // Default values
                config = config ?? new { };
                ApiKey = config.ApiKey ?? string.Empty;
                MerchantId = config.MerchantId ?? string.Empty;
                PublicKeyId = config.PublicKeyId ?? string.Empty;
                PrivateKeyId = config.PrivateKeyId ?? string.Empty;
                PayglocalPublicKey = config.PayglocalPublicKey ?? string.Empty;
                MerchantPrivateKey = config.MerchantPrivateKey ?? string.Empty;
                PayglocalEnv = config.PayglocalEnv ?? string.Empty;
                LogLevel = config.LogLevel ?? "info";
                TokenExpiration = config.TokenExpiration ?? 300000;

                // Validate payglocalEnv and set base URL
                if (string.IsNullOrEmpty(PayglocalEnv))
                {
                    throw new ArgumentException("Missing required configuration: PayglocalEnv for base URL");
                }

                string baseUrlEnv = PayglocalEnv.ToUpper();
                if (!BaseUrls.TryGetValue(baseUrlEnv, out string baseUrl))
                {
                    Logger.Error($"Invalid environment \"{baseUrlEnv}\" provided. Must be \"UAT\" or \"PROD\".");
                    throw new ArgumentException($"Invalid environment \"{baseUrlEnv}\" provided. Must be \"UAT\" or \"PROD\".");
                }
                BaseUrl = baseUrl;

                // Validate required fields
                if (string.IsNullOrEmpty(MerchantId))
                {
                    throw new ArgumentException("Missing required configuration: MerchantId");
                }

                // If API key is provided, token fields are not needed
                if (!string.IsNullOrEmpty(ApiKey))
                {
                    // API Key authentication - no need for token fields
                }
                else
                {
                    // Token authentication - require all token fields
                    if (string.IsNullOrEmpty(PublicKeyId) ||
                        string.IsNullOrEmpty(PrivateKeyId) ||
                        string.IsNullOrEmpty(PayglocalPublicKey) ||
                        string.IsNullOrEmpty(MerchantPrivateKey))
                    {
                        throw new ArgumentException("Missing required configuration for token authentication: PublicKeyId, PrivateKeyId, PayglocalPublicKey, MerchantPrivateKey");
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Configuration error: " + ex.Message);
                throw new ArgumentException(ex.Message ?? "Configuration initialization failed", ex);
            }
        }
    }
}