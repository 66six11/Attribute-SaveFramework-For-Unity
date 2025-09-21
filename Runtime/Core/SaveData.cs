using System;
using System.Collections.Generic;
using UnityEngine;

namespace SaveFramework.Runtime.Core
{
    /// <summary>
    /// 用于保存数据的容器，具有类型安全访问
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
                throw new ArgumentException("键不能为null或空", nameof(key));

            data[key] = value;
        }

        // /// <summary>
        // /// 获取给定键的值，默认值可选
        // /// </summary>
        // public T GetValue<T>(string key, T defaultValue = default(T))
        // {
        //     if (string.IsNullOrEmpty(key) || !data.TryGetValue(key, out var value))
        //         return defaultValue;
        //
        //     try
        //     {
        //         if (value is T directValue)
        //             return directValue;
        //         Debug.LogWarning($"{value}:type {defaultValue} :转换值为 {(T)Converters.FromJsonValue(value, typeof(T))}");
        //         // 尝试使用转换器系统进行转换
        //         return (T)Converters.FromJsonValue(value, typeof(T));
        //     }
        //     catch
        //     {
        //         Debug.LogWarning(" converter 转换失败");
        //         return defaultValue;
        //     }
        // }

        /// <summary>
        /// 获取给定键的值，默认值可选 (Type-safe generic version)
        /// </summary>
        public T GetValue<T>(string key, T defaultValue = default(T))
        {
            if (typeof(T) == typeof(System.Type))
                throw new InvalidOperationException("SaveFramework: 误用 GetValue<T> —— 推断到 T == System.Type。请改用非泛型重载 GetValue(key, targetType: typeof(YourType))。");

            if (string.IsNullOrEmpty(key) || !data.TryGetValue(key, out var value))
                return defaultValue;

            try
            {
                if (value is T directValue)
                    return directValue;
                
                // 尝试使用转换器系统进行转换
                return (T)Converters.FromJsonValue(value, typeof(T));
            }
            catch
            {
                Debug.LogWarning($"SaveFramework: 转换失败 key '{key}' 到类型 {typeof(T).Name}");
                return defaultValue;
            }
        }

        /// <summary>
        /// 获取给定键的值作为对象
        /// </summary>
        public object GetValue(string key, Type targetType, object defaultValue = null)
        {
            if (string.IsNullOrEmpty(key) || !data.TryGetValue(key, out var value))
                return defaultValue;
        
            try
            {
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
        /// 检查密钥是否存在
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