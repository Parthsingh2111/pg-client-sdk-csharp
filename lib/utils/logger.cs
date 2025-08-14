using System;
using System.Collections.Generic;
using System.Text.Json;

namespace Utils
{
    /// <summary>
    /// Enhanced Logger for PayGlocal SDK, providing structured logging with different levels and formatting.
    /// </summary>
    public class Logger
    {
        private readonly Dictionary<string, int> _levels = new Dictionary<string, int>
        {
            { "error", 0 },
            { "warn", 1 },
            { "info", 2 },
            { "debug", 3 }
        };
        private readonly string _level;

        /// <summary>
        /// Initializes a new instance of the Logger class.
        /// </summary>
        /// <param name="logger">The ILogger instance for logging (can be null for console logging).</param>
        /// <param name="level">The log level (error, warn, info, debug). Defaults to "info".</param>
        public Logger(object logger, string level = "info")
        {
            _level = NormalizeLevel(level);
        }

        /// <summary>
        /// Normalizes the log level to handle case variations.
        /// </summary>
        /// <param name="level">The log level to normalize.</param>
        /// <returns>The normalized log level.</returns>
        private string NormalizeLevel(string level)
        {
            string normalized = level.ToLower();
            if (_levels.ContainsKey(normalized))
            {
                return normalized;
            }

            Console.WriteLine($"[LOGGER] Invalid log level \"{level}\", defaulting to \"info\"");
            return "info";
        }

        /// <summary>
        /// Checks if a log level should be output.
        /// </summary>
        /// <param name="level">The log level to check.</param>
        /// <returns>True if the level should be logged, false otherwise.</returns>
        private bool ShouldLog(string level)
        {
            return _levels.TryGetValue(level, out int levelValue) && levelValue <= _levels[_level];
        }

        /// <summary>
        /// Gets the current timestamp in ISO format.
        /// </summary>
        /// <returns>The ISO timestamp.</returns>
        private string GetTimestamp()
        {
            return DateTime.UtcNow.ToString("O");
        }

        /// <summary>
        /// Formats a log message with timestamp and level.
        /// </summary>
        /// <param name="level">The log level.</param>
        /// <param name="message">The log message.</param>
        /// <param name="data">Optional additional data to log.</param>
        /// <returns>The formatted log message.</returns>
        private string FormatMessage(string level, string message, object data = null)
        {
            string timestamp = GetTimestamp();
            string prefix = $"[{timestamp}] [{level.ToUpper()}] [PAYGLOCAL-SDK]";

            if (data != null)
            {
                string dataJson = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
                return $"{prefix} {message} {dataJson}";
            }
            return $"{prefix} {message}";
        }

        /// <summary>
        /// Logs an error message (always shown).
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="error">Optional error object or additional data.</param>
        public void Error(string message, object error = null)
        {
            if (!ShouldLog("error"))
                return;

            if (error is Exception ex)
            {
                Console.Error.WriteLine(FormatMessage("error", message, new
                {
                    Error = ex.Message,
                    Stack = ex.StackTrace,
                    Name = ex.GetType().Name
                }));
            }
            else
            {
                Console.Error.WriteLine(FormatMessage("error", message, error));
            }
        }

        /// <summary>
        /// Logs a warning message.
        /// </summary>
        /// <param name="message">The warning message.</param>
        /// <param name="data">Optional additional data.</param>
        public void Warn(string message, object data = null)
        {
            if (!ShouldLog("warn"))
                return;

            Console.WriteLine(FormatMessage("warn", message, data));
        }

        /// <summary>
        /// Logs an info message.
        /// </summary>
        /// <param name="message">The info message.</param>
        /// <param name="data">Optional additional data.</param>
        public void Info(string message, object data = null)
        {
            if (!ShouldLog("info"))
                return;

            Console.WriteLine(FormatMessage("info", message, data));
        }

        /// <summary>
        /// Logs a debug message (only shown in debug mode).
        /// </summary>
        /// <param name="message">The debug message.</param>
        /// <param name="data">Optional additional data.</param>
        public void Debug(string message, object data = null)
        {
            if (!ShouldLog("debug"))
                return;

            Console.WriteLine(FormatMessage("debug", message, data));
        }

        /// <summary>
        /// Logs API request details.
        /// </summary>
        /// <param name="method">The HTTP method.</param>
        /// <param name="url">The request URL.</param>
        /// <param name="headers">Optional request headers (sensitive data will be masked).</param>
        /// <param name="data">Optional request data.</param>
        public void LogRequest(string method, string url, Dictionary<string, string> headers = null, object data = null)
        {
            if (!ShouldLog("debug"))
                return;

            // Mask sensitive headers
            var maskedHeaders = headers != null ? new Dictionary<string, string>(headers) : new Dictionary<string, string>();
            if (maskedHeaders.ContainsKey("x-gl-auth"))
            {
                maskedHeaders["x-gl-auth"] = "***MASKED***";
            }
            if (maskedHeaders.ContainsKey("x-gl-token-external"))
            {
                maskedHeaders["x-gl-token-external"] = "***MASKED***";
            }

            object logData = new
            {
                Headers = maskedHeaders,
                Data = data != null ? (data is string ? "[JWE/JWS Token]" : data) : null
            };

            Debug($"API Request: {method} {url}", logData);
        }

        /// <summary>
        /// Logs API response details.
        /// </summary>
        /// <param name="method">The HTTP method.</param>
        /// <param name="url">The request URL.</param>
        /// <param name="status">The response status code.</param>
        /// <param name="data">Optional response data.</param>
        public void LogResponse(string method, string url, int status, object data = null)
        {
            if (!ShouldLog("debug"))
                return;

            object logData = new
            {
                Status = status,
                Data = data ?? null
            };

            Debug($"API Response: {method} {url} - {status}", logData);
        }

        /// <summary>
        /// Logs configuration details (without sensitive data).
        /// </summary>
        /// <param name="config">The configuration object.</param>
        public void LogConfig(Core.Config config)
        {
            if (!ShouldLog("debug"))
                return;

            var safeConfig = new
            {
                config.MerchantId,
                config.PublicKeyId,
                config.PrivateKeyId,
                config.BaseUrl,
                config.LogLevel,
                config.TokenExpiration,
                HasApiKey = !string.IsNullOrEmpty(config.ApiKey),
                HasPayglocalPublicKey = !string.IsNullOrEmpty(config.PayglocalPublicKey),
                HasMerchantPrivateKey = !string.IsNullOrEmpty(config.MerchantPrivateKey)
            };

            Debug("SDK Configuration", safeConfig);
        }
    }
}