using System.Threading.Tasks;
using Core; // Assuming namespace for Config, Crypto
using Services; // Assuming namespace for Payment, Refund, Capture, Reversal, Status, SiUpdate
using Utils; // Assuming namespace for Logger

namespace PayGlocal
{
    /// <summary>
    /// PayGlocalClient for interacting with PayGlocal API.
    /// </summary>
    public class PayGlocalClient
    {
        private readonly Config _config;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the PayGlocalClient class.
        /// </summary>
        /// <param name="config">Configuration options.</param>
        public PayGlocalClient(Config config = null)
        {
            _config = config ?? new Config();
            Logger.LogConfig(_config);
            Logger.Info("PayGlocalClient initialized successfully");
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