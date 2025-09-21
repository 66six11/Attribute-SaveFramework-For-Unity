using System;
using System.Collections.Generic;

namespace SaveFramework.Runtime.Core
{
    /// <summary>
    /// Container for save data with type-safe access
    /// </summary>
    public class SaveData
    {
        private readonly Dictionary<string, object> data;

        public SaveData()
        {
            data = new Dictionary<string, object>();
        }

        public SaveData(Dictionary<string, object> existingData)
        {
            data = existingData ?? new Dictionary<string, object>();
        }

        /// <summary>
        /// Get the underlying data dictionary
        /// </summary>
        public Dictionary<string, object> Data => data;

        /// <summary>
        /// Set a value for the given key
        /// </summary>
        public void SetValue(string key, object value)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentException("Key cannot be null or empty", nameof(key));

            data[key] = value;
        }

        /// <summary>
        /// Get a value for the given key, with optional default
        /// </summary>
        public T GetValue<T>(string key, T defaultValue = default(T))
        {
            if (string.IsNullOrEmpty(key) || !data.ContainsKey(key))
                return defaultValue;

            try
            {
                var value = data[key];
                if (value is T directValue)
                    return directValue;

                // Try to convert using the Converters system
                return (T)Converters.FromJsonValue(value, typeof(T));
            }
            catch
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// Get a value for the given key as object
        /// </summary>
        public object GetValue(string key, Type targetType, object defaultValue = null)
        {
            if (string.IsNullOrEmpty(key) || !data.ContainsKey(key))
                return defaultValue;

            try
            {
                var value = data[key];
                if (targetType.IsInstanceOfType(value))
                    return value;

                // Try to convert using the Converters system
                return Converters.FromJsonValue(value, targetType);
            }
            catch
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// Check if a key exists
        /// </summary>
        public bool HasKey(string key)
        {
            return !string.IsNullOrEmpty(key) && data.ContainsKey(key);
        }

        /// <summary>
        /// Remove a key from the data
        /// </summary>
        public bool RemoveKey(string key)
        {
            if (string.IsNullOrEmpty(key))
                return false;

            return data.Remove(key);
        }

        /// <summary>
        /// Get all keys
        /// </summary>
        public ICollection<string> Keys => data.Keys;

        /// <summary>
        /// Clear all data
        /// </summary>
        public void Clear()
        {
            data.Clear();
        }

        /// <summary>
        /// Get the number of entries
        /// </summary>
        public int Count => data.Count;
    }
}