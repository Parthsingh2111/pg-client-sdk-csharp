using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Jose;
using Core;
using Utils;

namespace Helpers
{
    /// <summary>
    /// Provides cryptographic helper methods for generating JWE and JWS tokens using JOSE standards.
    /// </summary>
    public static class CryptoHelper
    {
        /// <summary>
        /// Converts a PEM key to an RSA key object.
        /// </summary>
        /// <param name="pem">PEM key string.</param>
        /// <param name="isPrivate">Indicates if the key is a private key.</param>
        /// <returns>RSA key object.</returns>
        /// <exception cref="ArgumentException">Thrown when the PEM key is invalid.</exception>
        public static RSA PemToKey(string pem, bool isPrivate = false)
        {
            if (string.IsNullOrEmpty(pem) || !pem.Contains("-----"))
            {
                throw new ArgumentException("PEM must be a non-empty string with ----- delimiters");
            }

            try
            {
                var rsa = RSA.Create();
                if (isPrivate)
                {
                    // Import PKCS#8 private key
                    pem = pem.Replace("-----BEGIN PRIVATE KEY-----", "").Replace("-----END PRIVATE KEY-----", "").Trim();
                    byte[] keyBytes = Convert.FromBase64String(pem);
                    rsa.ImportPkcs8PrivateKey(keyBytes, out _);
                }
                else
                {
                    // Import SPKI public key
                    pem = pem.Replace("-----BEGIN PUBLIC KEY-----", "").Replace("-----END PUBLIC KEY-----", "").Trim();
                    byte[] keyBytes = Convert.FromBase64String(pem);
                    rsa.ImportSubjectPublicKeyInfo(keyBytes, out _);
                }
                return rsa;
            }
            catch (Exception ex)
            {
                var logger = new Logger(null);
                logger.Error("PEM import error: " + ex.Message);
                throw new ArgumentException($"Crypto error: Invalid PEM format: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Generates a JWE token for the given payload using JOSE standards.
        /// </summary>
        /// <param name="payload">Payload to encrypt.</param>
        /// <param name="config">Configuration object.</param>
        /// <returns>JWE token as a string.</returns>
        /// <exception cref="ArgumentException">Thrown when encryption fails.</exception>
        public static Task<string> GenerateJWE(object payload, Config config)
        {
            try
            {
                long iat = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                long exp = iat + (config.TokenExpiration != 0 ? config.TokenExpiration : 300000); // Default 5 minutes
                
                // Get RSA public key for encryption
                RSA publicKey = PemToKey(config.PayglocalPublicKey, false);

                // Create JWE headers
                var headers = new Dictionary<string, object>
                {
                    { "alg", "RSA-OAEP-256" },
                    { "enc", "A128CBC-HS256" },
                    { "iat", iat.ToString() },
                    { "exp", exp.ToString() },
                    { "kid", config.PublicKeyId },
                    { "issued_by", config.MerchantId }
                };

                // Serialize payload
                string payloadJson = JsonSerializer.Serialize(payload);

                // Generate JWE using JOSE library
                string jwe = Jose.JWE.Encrypt(payloadJson, publicKey, JweAlgorithm.RSA_OAEP_256, JweEncryption.A128CBC_HS256, extraHeaders: headers);

                return Task.FromResult(jwe);
            }
            catch (Exception ex)
            {
                var logger = new Logger(null);
                logger.Error("JWE generation error: " + ex.Message);
                throw new ArgumentException($"Failed to generate JWE: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Generates a JWS token for a digestible string using JOSE standards.
        /// </summary>
        /// <param name="toDigest">Input string to hash.</param>
        /// <param name="config">Configuration object.</param>
        /// <returns>JWS token as a string.</returns>
        /// <exception cref="ArgumentException">Thrown when signing fails.</exception>
        public static Task<string> GenerateJWS(string toDigest, Config config)
        {
            try
            {
                long iat = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                long exp = iat + (config.TokenExpiration != 0 ? config.TokenExpiration : 300000); // Default 5 minutes

                // Create SHA-256 digest
                using (var sha256 = SHA256.Create())
                {
                    byte[] digestBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(toDigest));
                    string digestBase64 = Convert.ToBase64String(digestBytes);

                    // Create JWS payload
                    var payload = new
                    {
                        digest = digestBase64,
                        digestAlgorithm = "SHA-256",
                        exp,
                        iat = iat.ToString()
                    };

                    // Create JWS headers
                    var headers = new Dictionary<string, object>
                    {
                        { "issued_by", config.MerchantId },
                        { "alg", "RS256" },
                        { "kid", config.PrivateKeyId },
                        { "x_gl_merchantId", config.MerchantId },
                        { "x_gl_enc", "true" },
                        { "is_digested", "true" }
                    };

                    // Get RSA private key for signing
                    RSA privateKey = PemToKey(config.MerchantPrivateKey, true);

                    // Generate JWS using JOSE library
                    string jws = Jose.JWT.Encode(payload, privateKey, JwsAlgorithm.RS256, extraHeaders: headers);

                    return Task.FromResult(jws);
                }
            }
            catch (Exception ex)
            {
                var logger = new Logger(null);
                logger.Error("JWS generation error: " + ex.Message);
                throw new ArgumentException($"Failed to generate JWS: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Generates both JWE and JWS tokens for a given payload and operation.
        /// </summary>
        /// <param name="payload">Payload to encrypt.</param>
        /// <param name="config">Configuration object.</param>
        /// <param name="operation">Operation name (for logging).</param>
        /// <returns>TokenResult containing JWE and JWS tokens.</returns>
        public static async Task<TokenResult> GenerateTokens(object payload, Config config, string operation)
        {
            var logger = new Logger(null, config.LogLevel);
            
            try
            {
                logger.Debug($"Generating JOSE-compliant tokens for {operation}");
                
                string jwe = await GenerateJWE(payload, config);
                string jws = await GenerateJWS(jwe, config);
                
                logger.Debug($"Successfully generated JOSE tokens for {operation}");
                
                return new TokenResult { Jwe = jwe, Jws = jws };
            }
            catch (Exception ex)
            {
                logger.Error($"Token generation failed for {operation}", ex);
                throw;
            }
        }
    }
}