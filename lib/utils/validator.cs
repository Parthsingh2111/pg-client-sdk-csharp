using System;
using System.Linq;

namespace Helpers
{
    /// <summary>
    /// Provides methods for validating required fields in a data object.
    /// </summary>
    public static class FieldValidationHelper
    {
        /// <summary>
        /// Validates that all required fields are present in the data object.
        /// </summary>
        /// <param name="data">The data object to validate.</param>
        /// <param name="fields">The list of required field paths (e.g., "paymentData.totalAmount").</param>
        /// <exception cref="ArgumentException">Thrown when a required field is missing or null.</exception>
        public static void ValidateRequiredFields(object data, string[] fields)
        {
            if (data == null)
            {
                throw new ArgumentException("Data object cannot be null");
            }

            if (fields == null)
            {
                return; // No fields to validate
            }

            // Handle Dictionary<string, object> case (from validation helper)
            if (data is System.Collections.Generic.Dictionary<string, object> dict)
            {
                foreach (string field in fields)
                {
                    if (!dict.ContainsKey(field) || dict[field] == null)
                    {
                        throw new ArgumentException($"Missing required field: {field}");
                    }
                }
                return;
            }

            // Handle regular objects with properties
            foreach (string field in fields)
            {
                string[] keys = field.Split('.');
                object value = data;

                foreach (string key in keys)
                {
                    if (value == null)
                    {
                        throw new ArgumentException($"Missing required field: {field}");
                    }

                    var property = value.GetType().GetProperty(key);
                    if (property == null)
                    {
                        throw new ArgumentException($"Missing required field: {field}");
                    }

                    value = property.GetValue(value);
                    if (value == null)
                    {
                        throw new ArgumentException($"Missing required field: {field}");
                    }
                }
            }
        }
    }
}