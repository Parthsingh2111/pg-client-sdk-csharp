using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using Utils; // For Logger

namespace Helpers
{
    /// <summary>
    /// Provides methods for validating payloads with required fields, operation type, and conditional validation.
    /// </summary>
    public static class PayloadValidationHelper
    {
        /// <summary>
        /// Validates a payload with required fields, operation type, conditional fields, and schema validation.
        /// </summary>
        /// <param name="payload">The payload to validate.</param>
        /// <param name="options">Validation options.</param>
        /// <exception cref="ArgumentException">Thrown when validation fails.</exception>
        public static void ValidatePayload(object payload, PayloadValidationOptions? options = null)
        {
            try
            {
                if (payload == null)
                {
                    throw new ArgumentException("Payload cannot be null");
                }

                options ??= new PayloadValidationOptions();

                // Convert payload to JObject for nested field access
                string jsonPayload = Newtonsoft.Json.JsonConvert.SerializeObject(payload);
                JObject jObject = JObject.Parse(jsonPayload);

                // 1. Required Fields Validation
                if (options.RequiredFields?.Length > 0)
                {
                    var validationData = new Dictionary<string, object>();
                    foreach (string field in options.RequiredFields)
                    {
                        var value = GetFieldValue(jObject, field);
                        validationData[field] = value;
                    }
                    FieldValidationHelper.ValidateRequiredFields(validationData, options.RequiredFields);
                }

                // 2. Operation Type Validation
                if (options.OperationType != null)
                {
                    string field = options.OperationType.Field;
                    string[] validTypes = options.OperationType.ValidTypes ?? Array.Empty<string>();
                    var typeValue = GetFieldValue(jObject, field)?.ToString();

                    if (!validTypes.Contains(typeValue))
                    {
                        throw new ArgumentException(
                            $"Invalid value for {field}: {typeValue ?? "null"}. Expected one of: {string.Join(", ", validTypes)}"
                        );
                    }
                }

                // 3. Conditional Field Validation
                if (options.ConditionalValidation != null)
                {
                    string condition = options.ConditionalValidation.Condition;
                    string value = options.ConditionalValidation.Value;
                    string[] conditionalFields = options.ConditionalValidation.RequiredFields ?? Array.Empty<string>();
                    var actual = GetFieldValue(jObject, condition)?.ToString();

                    if (actual == value && conditionalFields.Length > 0)
                    {
                        var conditionalData = new Dictionary<string, object>();
                        foreach (string field in conditionalFields)
                        {
                            var val = GetFieldValue(jObject, field);
                            conditionalData[field] = val;
                        }
                        FieldValidationHelper.ValidateRequiredFields(conditionalData, conditionalFields);
                    }
                }

                // 4. Schema Validation
                if (options.ValidateSchema)
                {
                    ValidationHelper.ValidatePaycollectPayload(payload);
                }

                Logger.Debug("Validation passed");
            }
            catch (Exception ex)
            {
                Logger.Error("Validation failed", ex);
                throw new ArgumentException(ex.Message, ex);
            }
        }

        private static object? GetFieldValue(JObject jObject, string fieldPath)
        {
            var keys = fieldPath.Split('.');
            JToken current = jObject;

            foreach (string key in keys)
            {
                current = current[key];
                if (current == null)
                {
                    return null;
                }
            }

            return current.ToObject<object>();
        }
    }

    /// <summary>
    /// Represents options for payload validation.
    /// </summary>
    public class PayloadValidationOptions
    {
        public string[]? RequiredFields { get; set; }
        public bool ValidateSchema { get; set; } = true;
        public OperationTypeValidation? OperationType { get; set; }
        public ConditionalValidation? ConditionalValidation { get; set; }
    }

    /// <summary>
    /// Represents operation type validation settings.
    /// </summary>
    public class OperationTypeValidation
    {
        public string Field { get; set; } = string.Empty;
        public string[]? ValidTypes { get; set; }
    }

    /// <summary>
    /// Represents conditional validation settings.
    /// </summary>
    public class ConditionalValidation
    {
        public string Condition { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public string[]? RequiredFields { get; set; }
    }
}