using System.Threading.Tasks;
using Core;
using Services;
using Utils;

namespace PayGlocal
{
    /// <summary>
    /// PayGlocalClient for interacting with PayGlocal API.
    /// </summary>
    public class PayGlocalClient
    {
        private readonly Config _config;
        private readonly Logger _logger;

        /// <summary>
        /// Initializes a new instance of the PayGlocalClient class.
        /// </summary>
        /// <param name="config">Configuration options.</param>
        public PayGlocalClient(Config config = null)
        {
            _config = config ?? new Config();
            _logger = new Logger(null, _config.LogLevel);
            _logger.LogConfig(_config);
            _logger.Info("PayGlocalClient initialized successfully");
        }

        /// <summary>
        /// Initializes a new instance of the PayGlocalClient class with environment variable configuration.
        /// </summary>
        /// <param name="merchantId">Merchant ID from environment or parameter</param>
        /// <param name="apiKey">API Key from environment or parameter</param>
        /// <param name="environment">Environment (UAT/PROD) from environment or parameter</param>
        /// <param name="publicKeyId">Public Key ID for JWT authentication</param>
        /// <param name="privateKeyId">Private Key ID for JWT authentication</param>
        /// <param name="payglocalPublicKey">PayGlocal Public Key for JWT authentication</param>
        /// <param name="merchantPrivateKey">Merchant Private Key for JWT authentication</param>
        /// <param name="logLevel">Log level</param>
        /// <param name="tokenExpiration">Token expiration in milliseconds</param>
        public PayGlocalClient(
            string merchantId = null,
            string apiKey = null,
            string environment = null,
            string publicKeyId = null,
            string privateKeyId = null,
            string payglocalPublicKey = null,
            string merchantPrivateKey = null,
            string logLevel = null,
            int? tokenExpiration = null)
        {
            // Use environment variables with fallback to parameters
            var configData = new
            {
                MerchantId = merchantId ?? System.Environment.GetEnvironmentVariable("PAYGLOCAL_MERCHANT_ID"),
                ApiKey = apiKey ?? System.Environment.GetEnvironmentVariable("PAYGLOCAL_API_KEY"),
                PayglocalEnv = environment ?? System.Environment.GetEnvironmentVariable("PAYGLOCAL_ENV"),
                PublicKeyId = publicKeyId ?? System.Environment.GetEnvironmentVariable("PAYGLOCAL_PUBLIC_KEY_ID"),
                PrivateKeyId = privateKeyId ?? System.Environment.GetEnvironmentVariable("PAYGLOCAL_PRIVATE_KEY_ID"),
                PayglocalPublicKey = payglocalPublicKey ?? System.Environment.GetEnvironmentVariable("PAYGLOCAL_PUBLIC_KEY"),
                MerchantPrivateKey = merchantPrivateKey ?? System.Environment.GetEnvironmentVariable("PAYGLOCAL_PRIVATE_KEY"),
                LogLevel = logLevel ?? System.Environment.GetEnvironmentVariable("PAYGLOCAL_LOG_LEVEL") ?? "info",
                TokenExpiration = tokenExpiration ?? (int.TryParse(System.Environment.GetEnvironmentVariable("PAYGLOCAL_TOKEN_EXPIRATION"), out int exp) ? exp : 300000)
            };

            _config = new Config(configData);
            _logger = new Logger(null, _config.LogLevel);
            _logger.LogConfig(_config);
            _logger.Info("PayGlocalClient initialized successfully with environment configuration");
        }

        /// <summary>
        /// Initiates an API key payment.
        /// </summary>
        /// <param name="parameters">Payment parameters.</param>
        /// <returns>Task representing the asynchronous operation.</returns>
        public Task<object> InitiateApiKeyPayment(object parameters)
        {
            return PaymentService.InitiateApiKeyPayment(parameters, _config);
        }

        /// <summary>
        /// Initiates a JWT payment.
        /// </summary>
        /// <param name="parameters">Payment parameters.</param>
        /// <returns>Task representing the asynchronous operation.</returns>
        public Task<object> InitiateJwtPayment(object parameters)
        {
            return PaymentService.InitiateJwtPayment(parameters, _config);
        }

        /// <summary>
        /// Initiates an SI payment.
        /// </summary>
        /// <param name="parameters">Payment parameters.</param>
        /// <returns>Task representing the asynchronous operation.</returns>
        public Task<object> InitiateSiPayment(object parameters)
        {
            return PaymentService.InitiateSiPayment(parameters, _config);
        }

        /// <summary>
        /// Initiates an authentication payment.
        /// </summary>
        /// <param name="parameters">Payment parameters.</param>
        /// <returns>Task representing the asynchronous operation.</returns>
        public Task<object> InitiateAuthPayment(object parameters)
        {
            return PaymentService.InitiateAuthPayment(parameters, _config);
        }

        /// <summary>
        /// Initiates a refund.
        /// </summary>
        /// <param name="parameters">Refund parameters.</param>
        /// <returns>Task representing the asynchronous operation.</returns>
        public Task<object> InitiateRefund(object parameters)
        {
            return RefundService.InitiateRefund(parameters, _config);
        }

        /// <summary>
        /// Initiates a capture.
        /// </summary>
        /// <param name="parameters">Capture parameters.</param>
        /// <returns>Task representing the asynchronous operation.</returns>
        public Task<object> InitiateCapture(object parameters)
        {
            return CaptureService.InitiateCapture(parameters, _config);
        }

        /// <summary>
        /// Initiates an authentication reversal.
        /// </summary>
        /// <param name="parameters">Reversal parameters.</param>
        /// <returns>Task representing the asynchronous operation.</returns>
        public Task<object> InitiateAuthReversal(object parameters)
        {
            return ReversalService.InitiateAuthReversal(parameters, _config);
        }

        /// <summary>
        /// Initiates a status check.
        /// </summary>
        /// <param name="parameters">Status check parameters.</param>
        /// <returns>Task representing the asynchronous operation.</returns>
        public Task<object> InitiateCheckStatus(object parameters)
        {
            return StatusService.InitiateCheckStatus(parameters, _config);
        }

        /// <summary>
        /// Initiates a pause for SI.
        /// </summary>
        /// <param name="parameters">SI pause parameters.</param>
        /// <returns>Task representing the asynchronous operation.</returns>
        public Task<object> InitiatePauseSI(object parameters)
        {
            return SiUpdateService.InitiatePauseSI(parameters, _config);
        }

        /// <summary>
        /// Initiates an activation for SI.
        /// </summary>
        /// <param name="parameters">SI activation parameters.</param>
        /// <returns>Task representing the asynchronous operation.</returns>
        public Task<object> InitiateActivateSI(object parameters)
        {
            return SiUpdateService.InitiateActivateSI(parameters, _config);
        }
    }
}