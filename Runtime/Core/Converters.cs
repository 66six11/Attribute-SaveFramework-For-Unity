using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json.Linq;
using UnityEngine;
using SaveFramework.Runtime.Core.Conversion;

namespace SaveFramework.Runtime.Core
{
    /// <summary>
    /// Facade for conversion system - routes to modular converter registry
    /// </summary>
    public static class Converters
    {
        /// <summary>
        /// Convert a value to a JSON-serializable format
        /// </summary>
        public static object ToJsonValue(object value, Type type)
        {
            if (value == null)
                return null;

            // If value is already the target type, avoid unnecessary conversion
            if (type.IsInstanceOfType(value))
            {
                // For complex types with converters, still convert to JSON format
                var converter = ConverterRegistry.GetConverter(type);
                if (converter != null)
                    return converter.ToJsonValue(value);
            }

            // Handle enums
            if (type.IsEnum)
                return value.ToString();

            // Handle arrays and lists
            if (type.IsArray || (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>)))
            {
                var elementType = type.IsArray ? type.GetElementType() : type.GetGenericArguments()[0];
                var list = new List<object>();

                foreach (var item in (IEnumerable)value)
                {
                    list.Add(ToJsonValue(item, elementType));
                }

                return list.ToArray();
            }

            // Try registry converter
            var registryConverter = ConverterRegistry.GetConverter(type);
            if (registryConverter != null)
                return registryConverter.ToJsonValue(value);

            // Default: return as-is for primitive types
            return value;
        }

        /// <summary>
        /// Convert a JSON value back to the target type
        /// </summary>
        public static object FromJsonValue(object jsonValue, Type targetType)
        {
            if (jsonValue == null)
                return GetDefaultValue(targetType);

            // If already the target type, return directly to prevent double-conversion
            if (targetType.IsInstanceOfType(jsonValue))
                return jsonValue;

            // 解包 JValue
            if (jsonValue is JValue jv)
            {
                jsonValue = jv.Value;
                
                // Check again after unpacking
                if (targetType.IsInstanceOfType(jsonValue))
                    return jsonValue;
            }

            // 枚举（支持字符串名或数字值）
            if (targetType.IsEnum)
            {
                try
                {
                    if (jsonValue is string s)
                        return Enum.Parse(targetType, s, ignoreCase: true);

                    var num = Convert.ToInt32(jsonValue, CultureInfo.InvariantCulture);
                    return Enum.ToObject(targetType, num);
                }
                catch
                {
                    return GetDefaultValue(targetType);
                }
            }

            // 数组
            if (targetType.IsArray)
            {
                var elementType = targetType.GetElementType();

                if (jsonValue is JArray ja)
                {
                    var arr = Array.CreateInstance(elementType, ja.Count);
                    for (int i = 0; i < ja.Count; i++)
                    {
                        arr.SetValue(FromJsonValue(ja[i], elementType), i);
                    }
                    return arr;
                }

                if (jsonValue is IEnumerable enumerable && jsonValue is not string)
                {
                    var items = new List<object>();
                    foreach (var item in enumerable)
                        items.Add(item);

                    var arr = Array.CreateInstance(elementType, items.Count);
                    for (int i = 0; i < items.Count; i++)
                    {
                        arr.SetValue(FromJsonValue(items[i], elementType), i);
                    }
                    return arr;
                }

                // 单值 -> 单元素数组
                var single = Array.CreateInstance(elementType, 1);
                single.SetValue(FromJsonValue(jsonValue, elementType), 0);
                return single;
            }

            // List<T>
            if (targetType.IsGenericType && targetType.GetGenericTypeDefinition() == typeof(List<>))
            {
                var elementType = targetType.GetGenericArguments()[0];
                var list = (IList)Activator.CreateInstance(targetType);

                if (jsonValue is JArray ja)
                {
                    foreach (var item in ja)
                        list.Add(FromJsonValue(item, elementType));
                }
                else if (jsonValue is IEnumerable enumerable && jsonValue is not string)
                {
                    foreach (var item in enumerable)
                        list.Add(FromJsonValue(item, elementType));
                }
                else
                {
                    list.Add(FromJsonValue(jsonValue, elementType));
                }

                return list;
            }

            // Try registry converter
            var registryConverter = ConverterRegistry.GetConverter(targetType);
            if (registryConverter != null)
                return registryConverter.FromJsonValue(jsonValue);

            // 如果仍然是 JToken，尝试 ToObject
            if (jsonValue is JToken token)
            {
                try
                {
                    return token.ToObject(targetType);
                }
                catch
                {
                    // 回退
                }
            }

            // 基础类型转换
            try
            {
                return Convert.ChangeType(jsonValue, targetType, CultureInfo.InvariantCulture);
            }
            catch
            {
                return GetDefaultValue(targetType);
            }
        }

        /// <summary>
        /// Check if a type is supported for serialization
        /// </summary>
        public static bool IsSupported(Type type)
        {
            if (type.IsPrimitive || type == typeof(string) || type.IsEnum)
                return true;

            if (ConverterRegistry.HasConverter(type))
                return true;

            if (type.IsArray)
                return IsSupported(type.GetElementType());

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
                return IsSupported(type.GetGenericArguments()[0]);

            return false;
        }

        private static object GetDefaultValue(Type type)
        {
            return type.IsValueType ? Activator.CreateInstance(type) : null;
        }
    }
}