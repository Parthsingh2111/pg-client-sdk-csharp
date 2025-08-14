using System;
using System.Threading.Tasks;
using Core; // Assuming namespace for Config
using Utils; // Assuming namespace for Logger

namespace Helpers
{
    /// <summary>
    /// Provides methods for generating JWT tokens (JWE and JWS) for API requests.
    /// </summary>
    public static class TokenHelper
    {
        /// <summary>
        /// Generates JWE and JWS tokens for API requests.
        /// </summary>
        /// <param name="payload">The payload to encrypt in JWE.</param>
        /// <param name="config">Configuration object containing keys and settings.</param>
        /// <param name="operation">Operation name for logging and error messages.</param>
        /// <param name="digestInput">Optional input for JWS generation (defaults to JWE).</param>
        /// <returns>An object containing the JWE and JWS tokens.</returns>
        /// <exception cref="ArgumentException">Thrown when token generation fails.</exception>
        public static async Task<TokenResult> GenerateTokens(object payload, Config config, string operation, string digestInput = null)
        {
            try
            {
                var logger = new Logger();
                // Generate JWE
                string jwe;
                try
                {
                    jwe = await CryptoHelper.GenerateJWE(payload, config);
                }
                catch (Exception ex)
                {
                    logger.Error($"JWE generation failed for {operation}: {ex.Message}", ex);
                    throw new ArgumentException($"Failed to generate JWE for {operation}: {ex.Message}", ex);
                }

                // Generate JWS
                string jws;
                try
                {
                    string inputForJWS = digestInput ?? jwe;
                    jws = await CryptoHelper.GenerateJWS(inputForJWS, config);
                }
                catch (Exception ex)
                {
                    logger.Error($"JWS generation failed for {operation}: {ex.Message}", ex);
                    throw new ArgumentException($"Failed to generate JWS for {operation}: {ex.Message}", ex);
                }

                logger.Debug($"Tokens generated for {operation}");

                return new TokenResult { Jwe = jwe, Jws = jws };
            }
            catch (Exception ex)
            {
                Logger.Error($"Token generation failed for {operation}", ex);
                throw;
            }
        }
    }

    /// <summary>
    /// Represents the result of token generation.
    /// </summary>
    public class TokenResult
    {
        public string Jwe { get; set; }
        public string Jws { get; set; }
    }
}