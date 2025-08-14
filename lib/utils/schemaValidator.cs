using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using Utils;

namespace Helpers
{
    /// <summary>
    /// Provides methods for validating PayGlocal payment payloads against a JSON schema.
    /// </summary>
    public static class ValidationHelper
    {
        private static readonly JSchema PayglocalSchema = JSchema.Parse(@"{
            ""type"": ""object"",
            ""required"": [""merchantTxnId"", ""merchantCallbackURL"", ""paymentData""],
            ""properties"": {
                ""merchantTxnId"": { ""type"": ""string"" },
                ""merchantUniqueId"": { ""type"": [""string"", ""null""] },
                ""merchantCallbackURL"": { ""type"": ""string"" },
                ""captureTxn"": { ""type"": [""boolean"", ""null""] },
                ""gpiTxnTimeout"": { ""type"": ""string"" },
                ""paymentData"": {
                    ""type"": ""object"",
                    ""required"": [""totalAmount"", ""txnCurrency""],
                    ""properties"": {
                        ""totalAmount"": { ""type"": ""string"" },
                        ""txnCurrency"": { ""type"": ""string"" },
                        ""cardData"": {
                            ""type"": ""object"",
                            ""properties"": {
                                ""number"": { ""type"": ""string"" },
                                ""expiryMonth"": { ""type"": ""string"" },
                                ""expiryYear"": { ""type"": ""string"" },
                                ""securityCode"": { ""type"": ""string"" },
                                ""type"": { ""type"": ""string"" }
                            },
                            ""additionalProperties"": false
                        },
                        ""tokenData"": {
                            ""type"": ""object"",
                            ""properties"": {
                                ""number"": { ""type"": ""string"" },
                                ""expiryMonth"": { ""type"": ""string"" },
                                ""expiryYear"": { ""type"": ""string"" },
                                ""cryptogram"": { ""type"": ""string"" },
                                ""firstSix"": { ""type"": ""string"" },
                                ""lastFour"": { ""type"": ""string"" },
                                ""cardBrand"": { ""type"": ""string"" },
                                ""cardCountryCode"": { ""type"": ""string"" },
                                ""cardIssuerName"": { ""type"": ""string"" },
                                ""cardType"": { ""type"": ""string"" },
                                ""cardCategory"": { ""type"": ""string"" }
                            },
                            ""additionalProperties"": false
                        },
                        ""billingData"": {
                            ""type"": ""object"",
                            ""properties"": {
                                ""firstName"": { ""type"": ""string"" },
                                ""lastName"": { ""type"": ""string"" },
                                ""addressStreet1"": { ""type"": ""string"" },
                                ""addressStreet2"": { ""type"": [""string"", ""null""] },
                                ""addressCity"": { ""type"": ""string"" },
                                ""addressState"": { ""type"": ""string"" },
                                ""addressPostalCode"": { ""type"": ""string"" },
                                ""emailId"": { ""type"": ""string"" },
                                ""phoneNumber"": { ""type"": ""string"" }
                            },
                            ""additionalProperties"": false
                        }
                    },
                    ""additionalProperties"": false
                },
                ""standingInstruction"": {
                    ""type"": ""object"",
                    ""properties"": {
                        ""data"": {
                            ""type"": ""object"",
                            ""properties"": {
                                ""amount"": { ""type"": ""string"" },
                                ""maxAmount"": { ""type"": ""string"" },
                                ""numberOfPayments"": { ""type"": ""string"" },
                                ""frequency"": { ""type"": ""string"" },
                                ""type"": { ""type"": ""string"" },
                                ""startDate"": { ""type"": ""string"" }
                            },
                            ""additionalProperties"": false
                        }
                    },
                    ""additionalProperties"": false
                },
                ""riskData"": {
                    ""type"": ""object"",
                    ""properties"": {
                        ""orderData"": {
                            ""type"": ""array"",
                            ""items"": {
                                ""type"": ""object"",
                                ""properties"": {
                                    ""productDescription"": { ""type"": ""string"" },
                                    ""productSKU"": { ""type"": ""string"" },
                                    ""productType"": { ""type"": ""string"" },
                                    ""itemUnitPrice"": { ""type"": ""string"" },
                                    ""itemQuantity"": { ""type"": ""string"" }
                                },
                                ""additionalProperties"": false
                            }
                        },
                        ""customerData"": {
                            ""type"": ""object"",
                            ""properties"": {
                                ""customerAccountType"": { ""type"": ""string"" },
                                ""customerSuccessOrderCount"": { ""type"": ""string"" },
                                ""customerAccountCreationDate"": { ""type"": ""string"" },
                                ""merchantAssignedCustomerId"": { ""type"": ""string"" }
                            },
                            ""additionalProperties"": false
                        },
                        ""shippingData"": {
                            ""type"": ""object"",
                            ""properties"": {
                                ""firstName"": { ""type"": ""string"" },
                                ""lastName"": { ""type"": ""string"" },
                                ""addressStreet1"": { ""type"": ""string"" },
                                ""addressStreet2"": { ""type"": [""string"", ""null""] },
                                ""addressCity"": { ""type"": ""string"" },
                                ""addressState"": { ""type"": ""string"" },
                                ""addressStateCode"": { ""type"": ""string"" },
                                ""addressPostalCode"": { ""type"": ""string"" },
                                ""addressCountry"": { ""type"": ""string"" },
                                ""emailId"": { ""type"": ""string"" },
                                ""phoneNumber"": { ""type"": ""string"" }
                            },
                            ""additionalProperties"": false
                        },
                        ""flightData"": {
                            ""type"": ""array"",
                            ""items"": {
                                ""type"": ""object"",
                                ""properties"": {
                                    ""agentCode"": { ""type"": ""string"" },
                                    ""agentName"": { ""type"": ""string"" },
                                    ""ticketNumber"": { ""type"": ""string"" },
                                    ""reservationDate"": { ""type"": ""string"" },
                                    ""ticketIssueCity"": { ""type"": ""string"" },
                                    ""ticketIssueState"": { ""type"": ""string"" },
                                    ""ticketIssueCountry"": { ""type"": ""string"" },
                                    ""ticketIssuePostalCode"": { ""type"": ""string"" },
                                    ""reservationCode"": { ""type"": ""string"" },
                                    ""reservationSystem"": { ""type"": ""string"" },
                                    ""journeyType"": { ""type"": ""string"" },
                                    ""electronicTicket"": { ""type"": ""string"" },
                                    ""refundable"": { ""type"": ""string"" },
                                    ""ticketType"": { ""type"": ""string"" },
                                    ""legData"": {
                                        ""type"": ""array"",
                                        ""items"": {
                                            ""type"": ""object"",
                                            ""properties"": {
                                                ""routeId"": { ""type"": ""string"" },
                                                ""legId"": { ""type"": ""string"" },
                                                ""flightNumber"": { ""type"": ""string"" },
                                                ""departureDate"": { ""type"": ""string"" },
                                                ""departureAirportCode"": { ""type"": ""string"" },
                                                ""departureCity"": { ""type"": ""string"" },
                                                ""departureCountry"": { ""type"": ""string"" },
                                                ""arrivalDate"": { ""type"": ""string"" },
                                                ""arrivalAirportCode"": { ""type"": ""string"" },
                                                ""arrivalCity"": { ""type"": ""string"" },
                                                ""arrivalCountry"": { ""type"": ""string"" },
                                                ""carrierCode"": { ""type"": ""string"" },
                                                ""carrierName"": { ""type"": ""string"" },
                                                ""serviceClass"": { ""type"": ""string"" }
                                            },
                                            ""additionalProperties"": false
                                        }
                                    },
                                    ""passengerData"": {
                                        ""type"": ""array"",
                                        ""items"": {
                                            ""type"": ""object"",
                                            ""properties"": {
                                                ""title"": { ""type"": [""string"", ""null""] },
                                                ""firstName"": { ""type"": ""string"" },
                                                ""lastName"": { ""type"": ""string"" },
                                                ""dateOfBirth"": { ""type"": ""string"" },
                                                ""type"": { ""type"": ""string"" },
                                                ""email"": { ""type"": ""string"" },
                                                ""passportNumber"": { ""type"": ""string"" },
                                                ""passportCountry"": { ""type"": ""string"" },
                                                ""passportIssueDate"": { ""type"": ""string"" },
                                                ""passportExpiryDate"": { ""type"": ""string"" },
                                                ""referenceNumber"": { ""type"": ""string"" }
                                            },
                                            ""additionalProperties"": false
                                        }
                                    }
                                },
                                ""additionalProperties"": false
                            }
                        },
                        ""trainData"": {
                            ""type"": ""array"",
                            ""items"": {
                                ""type"": ""object"",
                                ""properties"": {
                                    ""ticketNumber"": { ""type"": ""string"" },
                                    ""reservationDate"": { ""type"": ""string"" },
                                    ""legData"": {
                                        ""type"": ""array"",
                                        ""items"": {
                                            ""type"": ""object"",
                                            ""properties"": {
                                                ""routeId"": { ""type"": ""string"" },
                                                ""legId"": { ""type"": ""string"" },
                                                ""trainNumber"": { ""type"": ""string"" },
                                                ""departureDate"": { ""type"": ""string"" },
                                                ""departureCity"": { ""type"": ""string"" },
                                                ""departureCountry"": { ""type"": ""string"" },
                                                ""arrivalDate"": { ""type"": ""string"" },
                                                ""arrivalCity"": { ""type"": ""string"" },
                                                ""arrivalCountry"": { ""type"": ""string"" }
                                            },
                                            ""additionalProperties"": false
                                        }
                                    },
                                    ""passengerData"": {
                                        ""type"": ""array"",
                                        ""items"": {
                                            ""type"": ""object"",
                                            ""properties"": {
                                                ""title"": { ""type"": [""string"", ""null""] },
                                                ""firstName"": { ""type"": ""string"" },
                                                ""lastName"": { ""type"": ""string"" },
                                                ""dateOfBirth"": { ""type"": ""string"" },
                                                ""type"": { ""type"": ""string"" },
                                                ""email"": { ""type"": ""string"" },
                                                ""passportNumber"": { ""type"": ""string"" },
                                                ""passportCountry"": { ""type"": ""string"" },
                                                ""passportIssueDate"": { ""type"": ""string"" },
                                                ""passportExpiryDate"": { ""type"": ""string"" },
                                                ""referenceNumber"": { ""type"": ""string"" }
                                            },
                                            ""additionalProperties"": false
                                        }
                                    }
                                },
                                ""additionalProperties"": false
                            }
                        },
                        ""busData"": {
                            ""type"": ""array"",
                            ""items"": {
                                ""type"": ""object"",
                                ""properties"": {
                                    ""ticketNumber"": { ""type"": ""string"" },
                                    ""reservationDate"": { ""type"": ""string"" },
                                    ""legData"": {
                                        ""type"": ""array"",
                                        ""items"": {
                                            ""type"": ""object"",
                                            ""properties"": {
                                                ""routeId"": { ""type"": ""string"" },
                                                ""legId"": { ""type"": ""string"" },
                                                ""busNumber"": { ""type"": ""string"" },
                                                ""departureDate"": { ""type"": ""string"" },
                                                ""departureCity"": { ""type"": ""string"" },
                                                ""departureCountry"": { ""type"": ""string"" },
                                                ""arrivalDate"": { ""type"": ""string"" },
                                                ""arrivalCity"": { ""type"": ""string"" },
                                                ""arrivalCountry"": { ""type"": ""string"" }
                                            },
                                            ""additionalProperties"": false
                                        }
                                    },
                                    ""passengerData"": {
                                        ""type"": ""array"",
                                        ""items"": {
                                            ""type"": ""object"",
                                            ""properties"": {
                                                ""title"": { ""type"": [""string"", ""null""] },
                                                ""firstName"": { ""type"": ""string"" },
                                                ""lastName"": { ""type"": ""string"" },
                                                ""dateOfBirth"": { ""type"": ""string"" },
                                                ""type"": { ""type"": ""string"" },
                                                ""email"": { ""type"": ""string"" },
                                                ""passportNumber"": { ""type"": ""string"" },
                                                ""passportCountry"": { ""type"": ""string"" },
                                                ""passportIssueDate"": { ""type"": ""string"" },
                                                ""passportExpiryDate"": { ""type"": ""string"" },
                                                ""referenceNumber"": { ""type"": ""string"" }
                                            },
                                            ""additionalProperties"": false
                                        }
                                    }
                                },
                                ""additionalProperties"": false
                            }
                        },
                        ""shipData"": {
                            ""type"": ""array"",
                            ""items"": {
                                ""type"": ""object"",
                                ""properties"": {
                                    ""ticketNumber"": { ""type"": ""string"" },
                                    ""reservationDate"": { ""type"": ""string"" },
                                    ""legData"": {
                                        ""type"": ""array"",
                                        ""items"": {
                                            ""type"": ""object"",
                                            ""properties"": {
                                                ""routeId"": { ""type"": ""string"" },
                                                ""legId"": { ""type"": ""string"" },
                                                ""shipNumber"": { ""type"": ""string"" },
                                                ""departureDate"": { ""type"": ""string"" },
                                                ""departureCity"": { ""type"": ""string"" },
                                                ""departureCountry"": { ""type"": ""string"" },
                                                ""arrivalDate"": { ""type"": ""string"" },
                                                ""arrivalCity"": { ""type"": ""string"" },
                                                ""arrivalCountry"": { ""type"": ""string"" }
                                            },
                                            ""additionalProperties"": false
                                        }
                                    },
                                    ""passengerData"": {
                                        ""type"": ""array"",
                                        ""items"": {
                                            ""type"": ""object"",
                                            ""properties"": {
                                                ""title"": { ""type"": [""string"", ""null""] },
                                                ""firstName"": { ""type"": ""string"" },
                                                ""lastName"": { ""type"": ""string"" },
                                                ""dateOfBirth"": { ""type"": ""string"" },
                                                ""type"": { ""type"": ""string"" },
                                                ""email"": { ""type"": ""string"" },
                                                ""passportNumber"": { ""type"": ""string"" },
                                                ""passportCountry"": { ""type"": ""string"" },
                                                ""passportIssueDate"": { ""type"": ""string"" },
                                                ""passportExpiryDate"": { ""type"": ""string"" },
                                                ""referenceNumber"": { ""type"": ""string"" }
                                            },
                                            ""additionalProperties"": false
                                        }
                                    }
                                },
                                ""additionalProperties"": false
                            }
                        },
                        ""cabData"": {
                            ""type"": ""array"",
                            ""items"": {
                                ""type"": ""object"",
                                ""properties"": {
                                    ""reservationDate"": { ""type"": ""string"" },
                                    ""legData"": {
                                        ""type"": ""array"",
                                        ""items"": {
                                            ""type"": ""object"",
                                            ""properties"": {
                                                ""routeId"": { ""type"": ""string"" },
                                                ""legId"": { ""type"": ""string"" },
                                                ""pickupDate"": { ""type"": ""string"" },
                                                ""departureCity"": { ""type"": ""string"" },
                                                ""departureCountry"": { ""type"": ""string"" },
                                                ""arrivalCity"": { ""type"": ""string"" },
                                                ""arrivalCountry"": { ""type"": ""string"" }
                                            },
                                            ""additionalProperties"": false
                                        }
                                    },
                                    ""passengerData"": {
                                        ""type"": ""array"",
                                        ""items"": {
                                            ""type"": ""object"",
                                            ""properties"": {
                                                ""title"": { ""type"": [""string"", ""null""] },
                                                ""firstName"": { ""type"": ""string"" },
                                                ""lastName"": { ""type"": ""string"" },
                                                ""dateOfBirth"": { ""type"": ""string"" },
                                                ""type"": { ""type"": ""string"" },
                                                ""email"": { ""type"": ""string"" },
                                                ""passportNumber"": { ""type"": ""string"" },
                                                ""passportCountry"": { ""type"": ""string"" },
                                                ""passportIssueDate"": { ""type"": ""string"" },
                                                ""passportExpiryDate"": { ""type"": ""string"" },
                                                ""referenceNumber"": { ""type"": ""string"" }
                                            },
                                            ""additionalProperties"": false
                                        }
                                    }
                                },
                                ""additionalProperties"": false
                            }
                        },
                        ""lodgingData"": {
                            ""type"": ""array"",
                            ""items"": {
                                ""type"": ""object"",
                                ""properties"": {
                                    ""checkInDate"": { ""type"": ""string"" },
                                    ""checkOutDate"": { ""type"": ""string"" },
                                    ""lodgingType"": { ""type"": ""string"" },
                                    ""lodgingName"": { ""type"": ""string"" },
                                    ""city"": { ""type"": ""string"" },
                                    ""country"": { ""type"": ""string"" },
                                    ""rating"": { ""type"": ""string"" },
                                    ""cancellationPolicy"": { ""type"": ""string"" },
                                    ""bookingPersonFirstName"": { ""type"": ""string"" },
                                    ""bookingPersonLastName"": { ""type"": ""string"" },
                                    ""bookingPersonEmailId"": { ""type"": ""string"" },
                                    ""bookingPersonCallingCode"": { ""type"": ""string"" },
                                    ""bookingPersonPhoneNumber"": { ""type"": ""string"" },
                                    ""rooms"": {
                                        ""type"": ""array"",
                                        ""items"": {
                                            ""type"": ""object"",
                                            ""properties"": {
                                                ""roomType"": { ""type"": ""string"" },
                                                ""roomCategory"": { ""type"": ""string"" },
                                                ""roomPrice"": { ""type"": ""string"" },
                                                ""numberOfGuests"": { ""type"": ""string"" },
                                                ""numberOfNights"": { ""type"": ""string"" },
                                                ""guestFirstName"": { ""type"": ""string"" },
                                                ""guestLastName"": { ""type"": ""string"" },
                                                ""guestEmail"": { ""type"": ""string"" }
                                            },
                                            ""additionalProperties"": false
                                        }
                                    }
                                },
                                ""additionalProperties"": false
                            }
                        }
                    },
                    ""additionalProperties"": false
                }
            },
            ""additionalProperties"": false
        }");

        /// <summary>
        /// Validates a payload against the PayGlocal schema.
        /// </summary>
        /// <param name="payload">The payload to validate.</param>
        /// <param name="options">Validation options including operation, required fields, and custom validation.</param>
        /// <returns>A validation result object.</returns>
        /// <exception cref="ArgumentException">Thrown when schema validation fails.</exception>
        public static ValidationResult ValidatePaycollectPayload(object payload, PayloadValidationOptions options = null)
        {
            var logger = new Logger(null);
            try
            {
                // Convert payload to JObject for validation
                string jsonPayload = JsonSerializer.Serialize(payload);
                JObject jObject = JObject.Parse(jsonPayload);

                // Validate against schema
                IList<ValidationError> errors;
                bool valid = jObject.IsValid(PayglocalSchema, out errors);

                if (!valid)
                {
                    var problematicFields = errors.Select(err =>
                    {
                        string field = string.IsNullOrEmpty(err.Path) ? "root" : err.Path;
                        string message;

                        if (err.ErrorType == SchemaValidationErrorType.AdditionalProperties)
                        {
                            message = $"Unrecognized field \"{err.Message.Split('\'')[1]}\"";
                        }
                        else if (err.ErrorType == SchemaValidationErrorType.InvalidType)
                        {
                            var value = field.Split('.').Aggregate(jObject, (current, key) => current?[key]);
                            message = $"Invalid type: expected {err.ExpectedType}, got {value?.Type}";
                        }
                        else
                        {
                            message = err.Message;
                        }

                        return new { Field = field, Error = message };
                    }).ToList();

                    logger.Error("Validation errors, verify the payload structure, problematic field", problematicFields);
                    throw new ArgumentException("Schema validation failed");
                }

                // Apply additional required fields validation from options
                if (options?.RequiredFields != null)
                {
                    var missingFields = options.RequiredFields
                        .Where(field => !IsFieldPresent(jObject, field))
                        .Select(field => new { Field = field, Error = $"Missing required field \"{field}\"" })
                        .ToList();

                    if (missingFields.Any())
                    {
                        logger.Error("Validation errors, verify the payload structure, problematic field", missingFields);
                        throw new ArgumentException("Required fields validation failed");
                    }
                }

                // Apply custom validation if provided
                if (options?.CustomValidation != null)
                {
                    options.CustomValidation(payload);
                }

                logger.Debug("Payload has passed payglocal schema validation for payCollect method");
                return new ValidationResult
                {
                    Message = "Payload is valid, payload has passed payglocal schema validation for payCollect method"
                };
            }
            catch (Exception ex)
            {
                logger.Error("Payload validation failed", ex);
                throw new ArgumentException($"Payload validation failed: {ex.Message}", ex);
            }
        }

        private static bool IsFieldPresent(JObject jObject, string fieldPath)
        {
            var tokens = fieldPath.Split('.');
            JToken current = jObject;

            foreach (var token in tokens)
            {
                current = current[token];
                if (current == null)
                    return false;
            }

            return true;
        }
    }

    /// <summary>
    /// Represents the result of a validation operation.
    /// </summary>
    public class ValidationResult
    {
        public string Message { get; set; } = string.Empty;
    }
}