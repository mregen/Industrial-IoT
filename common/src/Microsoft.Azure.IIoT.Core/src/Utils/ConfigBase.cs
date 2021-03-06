// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Utils {
    using Microsoft.Azure.IIoT.Exceptions;
    using Microsoft.Extensions.Configuration;
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Configuration base helper class
    /// </summary>
    public abstract class ConfigBase {

        /// <summary>
        /// Configuration
        /// </summary>
        public IConfiguration Configuration { get; }

        /// <summary>
        /// Configuration constructor
        /// </summary>
        /// <param name="configuration"></param>
        protected ConfigBase(IConfiguration configuration) {
            if (configuration == null) {
                configuration = new ConfigurationBuilder()
                    .Build();
            }
            Configuration = configuration;
        }

        /// <summary>
        /// Read variable and replace environment variable if needed
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        protected string GetStringOrDefault(string key, Func<string> defaultValue = null) {
            var value = Configuration.GetValue<string>(key);
            if (string.IsNullOrEmpty(value)) {
                if (defaultValue == null) {
                    return string.Empty;
                }
                return defaultValue.Invoke();
            }
            return value.Trim();
        }

        /// <summary>
        /// Read boolean
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        protected bool GetBoolOrDefault(string key, Func<bool> defaultValue = null) {
            var result = GetBoolOrNull(key);
            if (result.HasValue) {
                return result.Value;
            }
            return defaultValue?.Invoke() ?? false;
        }

        /// <summary>
        /// Read boolean
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        protected bool? GetBoolOrNull(string key) {
            var value = GetStringOrDefault(key, () => "").ToLowerInvariant();
            var knownTrue = new HashSet<string> { "true", "yes", "y", "1" };
            var knownFalse = new HashSet<string> { "false", "no", "n", "0" };
            if (knownTrue.Contains(value)) {
                return true;
            }
            if (knownFalse.Contains(value)) {
                return false;
            }
            return null;
        }

        /// <summary>
        /// Get time span
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        protected TimeSpan GetDurationOrDefault(string key,
            Func<TimeSpan> defaultValue = null) {
            if (!TimeSpan.TryParse(GetStringOrDefault(key), out var result)) {
                if (defaultValue != null) {
                    return defaultValue.Invoke();
                }
                throw new InvalidConfigurationException(
                    $"Unable to load timespan value for '{key}' from configuration.");
            }
            return result;
        }

        /// <summary>
        /// Get time span
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        protected TimeSpan? GetDurationOrNull(string key) {
            if (!TimeSpan.TryParse(GetStringOrDefault(key), out var result)) {
                // Try read as integer
                var value = GetIntOrNull(key);
                if (value.HasValue) {
                    return TimeSpan.FromMilliseconds(value.Value);
                }
                return null;
            }
            return result;
        }

        /// <summary>
        /// Read int
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        protected int GetIntOrDefault(string key, Func<int> defaultValue = null) {
            var value = GetIntOrNull(key);
            if (value.HasValue) {
                return value.Value;
            }
            return defaultValue?.Invoke() ?? 0;
        }

        /// <summary>
        /// Read int
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        protected int? GetIntOrNull(string key) {
            try {
                var value = GetStringOrDefault(key, null);
                if (string.IsNullOrEmpty(value)) {
                    return null;
                }
                return Convert.ToInt32(value);
            }
            catch {
                return null;
            }
        }
    }
}
