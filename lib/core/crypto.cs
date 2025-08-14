using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Core; // Assuming namespace for Config
using Utils; // Assuming namespace for Logger

namespace Helpers
{
    /// <summary>
    /// Provides cryptographic helper methods for generating JWE and JWS tokens.
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
                Logger.Error("PEM import error: " + ex.Message);
                throw new ArgumentException($"Crypto error: Invalid PEM format: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Generates a JWE token for the given payload.
        /// </summary>
        /// <param name="payload">Payload to encrypt.</param>
        /// <param name="config">Configuration object.</param>
        /// <returns>JWE token as a string.</returns>
        /// <exception cref="ArgumentException">Thrown when encryption fails.</exception>
        public static async Task<string> GenerateJWE(object payload, Config config)
        {
            try
            {
                long iat = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                long exp = iat + (config.TokenExpiration != 0 ? config.TokenExpiration : 300000); // Default 5 minutes
                RSA publicKey = PemToKey(config.PayglocalPublicKey, false);

                // Serialize payload to JSON
                string payloadStr = JsonSerializer.Serialize(payload);

                // Create protected header
                var header = new
                {
                    alg = "RSA-OAEP-256",
                    enc = "A128CBC-HS256",
                    iat = iat.ToString(),
                    exp = exp.ToString(),
                    kid = config.PublicKeyId,
                    issued_by = config.MerchantId
                };
                string headerJson = JsonSerializer.Serialize(header);
                string headerBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(headerJson));

                // Encrypt payload using RSA-OAEP and AES-128-CBC
                using (var aes = Aes.Create())
                {
                    aes.KeySize = 128;
                    aes.Mode = CipherMode.CBC;
                    aes.GenerateKey();
                    aes.GenerateIV();

                    byte[] contentKey = aes.Key;
                    byte[] iv = aes.IV;

                    // Encrypt content key with RSA-OAEP
                    byte[] encryptedKey = publicKey.Encrypt(contentKey, RSAEncryptionPadding.OaepSHA256);

                    // Encrypt payload with AES-128-CBC
                    byte[] payloadBytes = Encoding.UTF8.GetBytes(payloadStr);
                    using (var encryptor = aes.CreateEncryptor())
                    {
                        byte[] encryptedPayload = encryptor.TransformFinalBlock(payloadBytes, 0, payloadBytes.Length);

                        // Construct JWE: header.key.iv.ciphertext
                        string keyBase64 = Convert.ToBase64String(encryptedKey);
                        string ivBase64 = Convert.ToBase64String(iv);
                        string ciphertextBase64 = Convert.ToBase64String(encryptedPayload);

                        return $"{headerBase64}.{keyBase64}.{ivBase64}.{ciphertextBase64}";
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("JWE generation error: " + ex.Message);
                throw new ArgumentException($"Failed to generate JWE: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Generates a JWS token for a digestible string (e.g., JWE or request path).
        /// </summary>
        /// <param name="toDigest">Input string to hash.</param>
        /// <param name="config">Configuration object.</param>
        /// <returns>JWS token as a string.</returns>
        /// <exception cref="ArgumentException">Thrown when signing fails.</exception>
        public static async Task<string> GenerateJWS(string toDigest, Config config)
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

                    // Create payload
                    var payload = new
                    {
                        digest = digestBase64,
                        digestAlgorithm = "SHA-256",
                        exp,
                        iat = iat.ToString()
                    };
                    string payloadJson = JsonSerializer.Serialize(payload);
                    string payloadBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(payloadJson));

                    // Create header
                    var header = new
                    {
                        issued_by = config.MerchantId,
                        alg = "RS256",
                        kid = config.PrivateKeyId,
                        x_gl_merchantId = config.MerchantId,
                        x_gl_enc = "true",
                        is_digested = "true"
                    };
                    string headerJson = JsonSerializer.Serialize(header);
                    string headerBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(headerJson));

                    // Sign with RS256
                    RSA privateKey = PemToKey(config.MerchantPrivateKey, true);
                    using (var rsa = privateKey)
                    {
                        byte[] dataToSign = Encoding.UTF8.GetBytes($"{headerBase64}.{payloadBase64}");
                        byte[] signature = rsa.SignData(dataToSign, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
                        string signatureBase64 = Convert.ToBase64String(signature);

                        return $"{headerBase64}.{payloadBase64}.{signatureBase64}";
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("JWS generation error: " + ex.Message);
                throw new ArgumentException($"Failed to generate JWS: {ex.Message}", ex);
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

        /// <summary>
        /// Generates both JWE and JWS tokens for a given payload and operation.
        /// </summary>
        /// <param name="payload">Payload to encrypt.</param>
        /// <param name="config">Configuration object.</param>
        /// <param name="operation">Operation name (unused in this implementation).</param>
        /// <returns>TokenResult containing JWE and JWS tokens.</returns>
        public static async Task<TokenResult> GenerateTokens(object payload, Config config, string operation)
        {
            string jwe = await GenerateJWE(payload, config);
            string jws = await GenerateJWS(jwe, config);
            return new TokenResult { Jwe = jwe, Jws = jws };
        }
    }
}