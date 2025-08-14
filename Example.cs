using System;
using System.Threading.Tasks;
using PayGlocal;

namespace PayGlocal.Example
{
    /// <summary>
    /// Example demonstrating PayGlocal SDK usage
    /// </summary>
    public class PayGlocalExample
    {
        public static async Task Main(string[] args)
        {
            try
            {
                // Option 1: Initialize with environment variables
                var client = new PayGlocalClient();

                // Option 2: Initialize with explicit parameters
                // var client = new PayGlocalClient(
                //     merchantId: "your-merchant-id",
                //     apiKey: "your-api-key",
                //     environment: "UAT", // or "PROD"
                //     logLevel: "debug"
                // );

                // Example payment parameters
                var paymentParams = new
                {
                    merchantTxnId = "TXN" + DateTime.Now.Ticks,
                    merchantCallbackURL = "https://your-callback-url.com/webhook",
                    paymentData = new
                    {
                        totalAmount = "100.00",
                        txnCurrency = "INR",
                        billingData = new
                        {
                            firstName = "John",
                            lastName = "Doe",
                            emailId = "john.doe@example.com",
                            phoneNumber = "9876543210",
                            addressStreet1 = "123 Main St",
                            addressCity = "Mumbai",
                            addressState = "Maharashtra",
                            addressPostalCode = "400001"
                        }
                    }
                };

                Console.WriteLine("Initiating payment...");
                
                // Initiate API Key Payment
                var response = await client.InitiateApiKeyPayment(paymentParams);
                Console.WriteLine($"Payment Response: {response}");

                // Example: Check transaction status
                var statusParams = new
                {
                    gid = "your-transaction-gid" // Replace with actual GID from payment response
                };

                // var statusResponse = await client.InitiateCheckStatus(statusParams);
                // Console.WriteLine($"Status Response: {statusResponse}");

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
            }
        }
    }
} 