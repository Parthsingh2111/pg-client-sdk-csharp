# PayGlocal C# SDK

Official C# SDK for PayGlocal Payment Gateway API integration.

## Features

- **Multiple Payment Types**: API Key, JWT, Standing Instructions, Authentication payments
- **Transaction Management**: Refunds, Captures, Status checks, Reversals
- **Standing Instructions**: Pause and activate recurring payments
- **Comprehensive Validation**: Schema validation and business logic validation
- **Security**: JOSE-compliant JWT/JWE token generation with RSA encryption
- **Environment Support**: UAT and Production environments
- **Logging**: Configurable logging with sensitive data masking

## Installation

### Using .NET CLI
```bash
dotnet add package PayGlocal.SDK
```

### Using Package Manager
```bash
Install-Package PayGlocal.SDK
```

## Quick Start

### 1. Environment Variables Configuration (Recommended)

Set the following environment variables:

```bash
# Required
PAYGLOCAL_MERCHANT_ID=your-merchant-id
PAYGLOCAL_ENV=UAT  # or PROD

# For API Key Authentication
PAYGLOCAL_API_KEY=your-api-key

# For JWT Authentication (alternative to API Key)
PAYGLOCAL_PUBLIC_KEY_ID=your-public-key-id
PAYGLOCAL_PRIVATE_KEY_ID=your-private-key-id
PAYGLOCAL_PUBLIC_KEY=your-payglocal-public-key
PAYGLOCAL_PRIVATE_KEY=your-merchant-private-key

# Optional
PAYGLOCAL_LOG_LEVEL=info  # error, warn, info, debug
PAYGLOCAL_TOKEN_EXPIRATION=300000  # in milliseconds
```

### 2. Initialize Client

```csharp
using PayGlocal;

// Option 1: Using environment variables
var client = new PayGlocalClient();

// Option 2: Using explicit parameters
var client = new PayGlocalClient(
    merchantId: "your-merchant-id",
    apiKey: "your-api-key",
    environment: "UAT", // or "PROD"
    logLevel: "debug"
);
```

### 3. Make a Payment

```csharp
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

try
{
    var response = await client.InitiateApiKeyPayment(paymentParams);
    Console.WriteLine($"Payment Response: {response}");
}
catch (Exception ex)
{
    Console.WriteLine($"Payment failed: {ex.Message}");
}
```

## API Reference

### Payment Operations

#### API Key Payment
```csharp
var response = await client.InitiateApiKeyPayment(parameters);
```

#### JWT Payment
```csharp
var response = await client.InitiateJwtPayment(parameters);
```

#### Standing Instruction Payment
```csharp
var siParams = new
{
    merchantTxnId = "SI_TXN_001",
    merchantCallbackURL = "https://callback.url",
    paymentData = new
    {
        totalAmount = "500.00",
        txnCurrency = "INR"
    },
    standingInstruction = new
    {
        data = new
        {
            numberOfPayments = "12",
            frequency = "MONTHLY",
            type = "FIXED",
            amount = "500.00",
            startDate = "2024-01-15"
        }
    }
};

var response = await client.InitiateSiPayment(siParams);
```

#### Authentication Payment
```csharp
var authParams = new
{
    merchantTxnId = "AUTH_TXN_001",
    merchantCallbackURL = "https://callback.url",
    captureTxn = false, // Must be false for auth payment
    paymentData = new
    {
        totalAmount = "200.00",
        txnCurrency = "INR"
    }
};

var response = await client.InitiateAuthPayment(authParams);
```

### Transaction Management

#### Check Status
```csharp
var statusParams = new { gid = "transaction-gid" };
var response = await client.InitiateCheckStatus(statusParams);
```

#### Refund
```csharp
var refundParams = new
{
    gid = "transaction-gid",
    refundAmount = "50.00"
};
var response = await client.InitiateRefund(refundParams);
```

#### Capture
```csharp
var captureParams = new
{
    gid = "auth-transaction-gid",
    captureAmount = "150.00"
};
var response = await client.InitiateCapture(captureParams);
```

#### Authorization Reversal
```csharp
var reversalParams = new
{
    gid = "auth-transaction-gid",
    reversalAmount = "200.00"
};
var response = await client.InitiateAuthReversal(reversalParams);
```

### Standing Instruction Management

#### Pause SI
```csharp
var pauseParams = new
{
    siId = "si-id",
    action = "pause"
};
var response = await client.InitiatePauseSI(pauseParams);
```

#### Activate SI
```csharp
var activateParams = new
{
    siId = "si-id",
    action = "activate"
};
var response = await client.InitiateActivateSI(activateParams);
```

## Configuration

### Environment Variables

| Variable | Required | Description |
|----------|----------|-------------|
| `PAYGLOCAL_MERCHANT_ID` | Yes | Your merchant ID |
| `PAYGLOCAL_ENV` | Yes | Environment: `UAT` or `PROD` |
| `PAYGLOCAL_API_KEY` | Yes* | API key for authentication |
| `PAYGLOCAL_PUBLIC_KEY_ID` | Yes** | Public key ID for JWT auth |
| `PAYGLOCAL_PRIVATE_KEY_ID` | Yes** | Private key ID for JWT auth |
| `PAYGLOCAL_PUBLIC_KEY` | Yes** | PayGlocal public key for JWT auth |
| `PAYGLOCAL_PRIVATE_KEY` | Yes** | Merchant private key for JWT auth |
| `PAYGLOCAL_LOG_LEVEL` | No | Log level: `error`, `warn`, `info`, `debug` |
| `PAYGLOCAL_TOKEN_EXPIRATION` | No | Token expiration in milliseconds |

*Required for API Key authentication
**Required for JWT authentication

### Log Levels

- `error`: Only error messages
- `warn`: Warnings and errors
- `info`: General information (default)
- `debug`: Detailed debugging information

## Error Handling

The SDK throws `ArgumentException` for validation errors and `HttpRequestException` for API communication errors.

```csharp
try
{
    var response = await client.InitiateApiKeyPayment(parameters);
}
catch (ArgumentException ex)
{
    // Validation error
    Console.WriteLine($"Validation Error: {ex.Message}");
}
catch (HttpRequestException ex)
{
    // API communication error
    Console.WriteLine($"API Error: {ex.Message}");
}
catch (Exception ex)
{
    // Other errors
    Console.WriteLine($"Unexpected Error: {ex.Message}");
}
```

## Requirements

- .NET 6.0 or later
- jose-jwt 4.1.0+ (JOSE-compliant JWT/JWE generation)
- Newtonsoft.Json 13.0.3+
- Newtonsoft.Json.Schema 3.0.15+

## Support

For support and questions, please contact PayGlocal support team.

## License

This project is licensed under the MIT License. 