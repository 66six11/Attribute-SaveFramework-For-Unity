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
        /// 获取基础数据字典
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
        /// 从数据中删除key
        /// </summary>
        public bool RemoveKey(string key)
        {
            if (string.IsNullOrEmpty(key))
                return false;

            return data.Remove(key);
        }

        /// <summary>
        ///获取所有keys
        /// </summary>
        public ICollection<string> Keys => data.Keys;

        /// <summary>
        ///清除所有数据
        /// </summary>
        public void Clear()
        {
            data.Clear();
        }

        /// <summary>
        ///获取条目数量
        /// </summary>
        public int Count => data.Count;
    }
}