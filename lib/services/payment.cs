using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Helpers; // Assuming namespace for TokenHelper, ValidationHelper, ApiRequestHelper, HeaderHelper
using Constants; // Assuming namespace for Endpoints
using Utils; // Assuming namespace for Logger

namespace Services
{
    /// <summary>
    /// Provides methods to initiate various types of payments.
    /// </summary>
    public static class PaymentService
    {
        /// <summary>
        /// Main payment initiator logic.
        /// </summary>
        /// <param name="parameters">Payment parameters.</param>
        /// <param name="config">Configuration object.</param>
        /// <param name="options">Optional payment options.</param>
        /// <returns>Task representing the payment response.</returns>
        private static async Task<object> InitiatePayment(object parameters, Config config, PaymentOptions options = null)
        {
            options = options ?? new PaymentOptions();
            string operation = options.Operation ?? "payment";
            string endpoint = options.Endpoint ?? Endpoints.Payment.Initiate;
            string[] requiredFields = options.RequiredFields ?? Array.Empty<string>();
            bool useJwt = options.UseJwt ?? true;
            Action<object> customValidation = options.CustomValidation;
            Dictionary<string, string> customHeaders = options.CustomHeaders ?? new Dictionary<string, string>();

            Logger.Info($"Initiating {operation}", new
            {
                MerchantTxnId = GetProperty(parameters, "merchantTxnId"),
                UseJwt = useJwt,
                Endpoint = endpoint
            });

            // 1-3. Comprehensive Validation (Schema, Custom, Required Fields)
            ValidationHelper.ValidatePayload(parameters, new ValidationOptions
            {
                Operation = operation,
                RequiredFields = requiredFields,
                CustomValidation = customValidation
            });

            // 4. Custom Validation Processing (Payment-specific business logic)
            if (customValidation != null)
            {
                Logger.Debug($"Executing custom validation for {operation}");
                customValidation(parameters);
            }

            // 5. Token Generation and Header Building
            object requestData;
            Dictionary<string, string> headers;
            if (useJwt)
            {
                var tokens = await TokenHelper.GenerateTokens(parameters, config, operation);
                requestData = tokens.Jwe;
                headers = HeaderHelper.BuildJwtHeaders(tokens.Jws, customHeaders);
            }
            else
            {
                requestData = parameters;
                headers = HeaderHelper.BuildApiKeyHeaders(config.ApiKey, customHeaders);
            }

            // 6. API Call with Response Processing
            var response = await ApiRequestHelper.MakePaymentRequest(new RequestOptions
            {
                Method = "POST",
                BaseUrl = config.BaseUrl,
                Endpoint = endpoint,
                RequestData = requestData,
                Headers = headers,
                Operation = operation
            });

            Logger.Info($"{operation} completed successfully", new
            {
                MerchantTxnId = GetProperty(parameters, "merchantTxnId"),
                ResponseStatus = GetProperty(response, "status") ?? "unknown"
            });

            return response;
        }

        /// <summary>
        /// Initiates an API key-based payment.
        /// </summary>
        /// <param name="parameters">Payment parameters including merchantTxnId, paymentData, and merchantCallbackURL.</param>
        /// <param name="config">Configuration object.</param>
        /// <returns>Task representing the payment response.</returns>
        public static Task<object> InitiateApiKeyPayment(object parameters, Config config)
        {
            return InitiatePayment(parameters, config, new PaymentOptions
            {
                Operation = "API key payment",
                UseJwt = false,
                RequiredFields = new[]
                {
                    "merchantTxnId",
                    "paymentData",
                    "merchantCallbackURL",
                    "paymentData.totalAmount",
                    "paymentData.txnCurrency"
                },
                CustomHeaders = new Dictionary<string, string>
                {
                    { "x-gl-auth", config.ApiKey }
                }
            });
        }

        /// <summary>
        /// Initiates a JWT-based payment.
        /// </summary>
        /// <param name="parameters">Payment parameters including merchantTxnId, paymentData, and merchantCallbackURL.</param>
        /// <param name="config">Configuration object.</param>
        /// <returns>Task representing the payment response.</returns>
        public static Task<object> InitiateJwtPayment(object parameters, Config config)
        {
            return InitiatePayment(parameters, config, new PaymentOptions
            {
                Operation = "JWT payment",
                RequiredFields = new[]
                {
                    "merchantTxnId",
                    "paymentData",
                    "merchantCallbackURL",
                    "paymentData.totalAmount",
                    "paymentData.txnCurrency"
                }
            });
        }

        /// <summary>
        /// Initiates a Standing Instruction (SI) payment.
        /// </summary>
        /// <param name="parameters">Payment parameters including merchantTxnId, paymentData, standingInstruction, and merchantCallbackURL.</param>
        /// <param name="config">Configuration object.</param>
        /// <returns>Task representing the payment response.</returns>
        public static Task<object> InitiateSiPayment(object parameters, Config config)
        {
            return InitiatePayment(parameters, config, new PaymentOptions
            {
                Operation = "SI payment",
                RequiredFields = new[]
                {
                    "merchantTxnId",
                    "paymentData",
                    "standingInstruction",
                    "merchantCallbackURL",
                    "paymentData.totalAmount",
                    "paymentData.txnCurrency",
                    "standingInstruction.data",
                    "standingInstruction.data.numberOfPayments",
                    "standingInstruction.data.frequency",
                    "standingInstruction.data.type"
                },
                CustomValidation = (paramsObj) =>
                {
                    var standingInstruction = GetProperty(paramsObj, "standingInstruction");
                    var siData = GetProperty(standingInstruction, "data");
                    var siType = GetProperty(siData, "type")?.ToString();

                    // Validate SI type
                    if (siType != "FIXED" && siType != "VARIABLE")
                    {
                        throw new Exception("Invalid SI type. Must be either FIXED or VARIABLE");
                    }

                    // Conditional validations for SI
                    if (siType == "FIXED" && GetProperty(siData, "startDate") == null)
                    {
                        throw new Exception("startDate is required for FIXED SI type");
                    }

                    if (siType == "VARIABLE" && GetProperty(siData, "startDate") != null)
                    {
                        throw new Exception("startDate should not be included for VARIABLE SI type");
                    }

                    if (GetProperty(siData, "amount") == null && GetProperty(siData, "maxAmount") == null)
                    {
                        throw new Exception("Either amount or maxAmount is required for standingInstruction.data");
                    }
                }
            });
        }

        /// <summary>
        /// Initiates an authentication payment.
        /// </summary>
        /// <param name="parameters">Payment parameters including merchantTxnId, paymentData, captureTxn, and merchantCallbackURL.</param>
        /// <param name="config">Configuration object.</param>
        /// <returns>Task representing the payment response.</returns>
        public static Task<object> InitiateAuthPayment(object parameters, Config config)
        {
            return InitiatePayment(parameters, config, new PaymentOptions
            {
                Operation = "Auth payment",
                RequiredFields = new[]
                {
                    "merchantTxnId",
                    "paymentData",
                    "captureTxn",
                    "merchantCallbackURL",
                    "paymentData.totalAmount",
                    "paymentData.txnCurrency"
                },
                CustomValidation = (paramsObj) =>
                {
                    bool captureTxn = Convert.ToBoolean(GetProperty(paramsObj, "captureTxn"));
                    if (captureTxn)
                    {
                        throw new Exception("captureTxn should be false for Auth payment");
                    }
                }
            });
        }

        // Helper method to safely access dynamic properties
        private static object GetProperty(object obj, string propertyName)
        {
            if (obj == null) return null;
            var prop = obj.GetType().GetProperty(propertyName);
            return prop?.GetValue(obj);
        }
    }

    /// <summary>
    /// Represents payment initiation options.
    /// </summary>
    public class PaymentOptions
    {
        public string Operation { get; set; }
        public string Endpoint { get; set; }
        public string[] RequiredFields { get; set; }
        public bool? UseJwt { get; set; }
        public Action<object> CustomValidation { get; set; }
        public Dictionary<string, string> CustomHeaders { get; set; }
    }

    /// <summary>
    /// Represents validation options for payload validation.
    /// </summary>
    public class ValidationOptions
    {
        public string Operation { get; set; }
        public string[] RequiredFields { get; set; }
        public Action<object> CustomValidation { get; set; }
    }

    /// <summary>
    /// Represents options for making a payment request.
    /// </summary>
    public class RequestOptions
    {
        public string Method { get; set; }
        public string BaseUrl { get; set; }
        public string Endpoint { get; set; }
        public object RequestData { get; set; }
        public Dictionary<string, string> Headers { get; set; }
        public string Operation { get; set; }
    }
}
