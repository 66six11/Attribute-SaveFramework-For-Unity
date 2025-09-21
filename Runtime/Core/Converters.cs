using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace SaveFramework.Runtime.Core
{
    /// <summary>
    /// Handles conversion between supported types and JSON-serializable values
    /// </summary>
    public static class Converters
    {
        private static readonly Dictionary<Type, Func<object, object>> ToJsonConverters = new Dictionary<Type, Func<object, object>>
        {
            { typeof(Vector2), obj => Vector2ToArray((Vector2)obj) },
            { typeof(Vector3), obj => Vector3ToArray((Vector3)obj) },
            { typeof(Vector4), obj => Vector4ToArray((Vector4)obj) },
            { typeof(Quaternion), obj => QuaternionToArray((Quaternion)obj) },
            { typeof(Color), obj => ColorToArray((Color)obj) },
        };

        private static readonly Dictionary<Type, Func<object, object>> FromJsonConverters = new Dictionary<Type, Func<object, object>>
        {
            { typeof(Vector2), obj => ArrayToVector2(obj) },
            { typeof(Vector3), obj => ArrayToVector3(obj) },
            { typeof(Vector4), obj => ArrayToVector4(obj) },
            { typeof(Quaternion), obj => ArrayToQuaternion(obj) },
            { typeof(Color), obj => ArrayToColor(obj) },
        };

        /// <summary>
        /// Convert a value to a JSON-serializable format
        /// </summary>
        public static object ToJsonValue(object value, Type type)
        {
            if (value == null)
                return null;

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

            // Handle custom converters
            if (ToJsonConverters.TryGetValue(type, out var converter))
                return converter(value);

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

            // 解包 JValue
            if (jsonValue is JValue jv)
            {
                jsonValue = jv.Value;
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

            // 数组（不再把非 JArray 强制封装到 JArray，避免 Color 之类类型触发 JSON.NET 的 JValue 创建失败）
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

            // 自定义转换（Vector/Quaternion/Color）
            if (FromJsonConverters.TryGetValue(targetType, out var custom))
                return custom(jsonValue);

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

            if (ToJsonConverters.ContainsKey(type))
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

        #region Vector/Quaternion/Color Converters
        private static float[] Vector2ToArray(Vector2 v) => new float[] { v.x, v.y };
        private static float[] Vector3ToArray(Vector3 v) => new float[] { v.x, v.y, v.z };
        private static float[] Vector4ToArray(Vector4 v) => new float[] { v.x, v.y, v.z, v.w };
        private static float[] QuaternionToArray(Quaternion q) => new float[] { q.x, q.y, q.z, q.w };
        private static float[] ColorToArray(Color c) => new float[] { c.r, c.g, c.b, c.a };

        private static Vector2 ArrayToVector2(object obj)
        {
            var arr = GetFloatArray(obj);
            return arr.Length >= 2 ? new Vector2(arr[0], arr[1]) : Vector2.zero;
        }

        private static Vector3 ArrayToVector3(object obj)
        {
            var arr = GetFloatArray(obj);
            return arr.Length >= 3 ? new Vector3(arr[0], arr[1], arr[2]) : Vector3.zero;
        }

        private static Vector4 ArrayToVector4(object obj)
        {
            var arr = GetFloatArray(obj);
            return arr.Length >= 4 ? new Vector4(arr[0], arr[1], arr[2], arr[3]) : Vector4.zero;
        }

        private static Quaternion ArrayToQuaternion(object obj)
        {
            var arr = GetFloatArray(obj);
            return arr.Length >= 4 ? new Quaternion(arr[0], arr[1], arr[2], arr[3]) : Quaternion.identity;
        }

        private static Color ArrayToColor(object obj)
        {
            var arr = GetFloatArray(obj);
            return arr.Length >= 4 ? new Color(arr[0], arr[1], arr[2], arr[3]) : Color.white;
        }

        // 强化版：支持 JArray / 各种数组类型 / IEnumerable 数字 / 单值
        private static float[] GetFloatArray(object obj)
        {
            // 直接是 Unity 类型
            if (obj is Color c)            return new[] { c.r, c.g, c.b, c.a };
            if (obj is Vector2 v2)         return new[] { v2.x, v2.y };
            if (obj is Vector3 v3)         return new[] { v3.x, v3.y, v3.z };
            if (obj is Vector4 v4)         return new[] { v4.x, v4.y, v4.z, v4.w };
            if (obj is Quaternion q)       return new[] { q.x, q.y, q.z, q.w };
            
            // JArray
            if (obj is JArray jArray)
            {
                var result = new float[jArray.Count];
                for (int i = 0; i < jArray.Count; i++)
                {
                    // 兼容 int/double/string
                    result[i] = Convert.ToSingle(jArray[i].ToObject<object>(), CultureInfo.InvariantCulture);
                }
                return result;
            }

            // 任何托管数组（float[]/double[]/object[]等）
            if (obj is Array arr)
            {
                var len = arr.Length;
                var result = new float[len];
                for (int i = 0; i < len; i++)
                {
                    var v = arr.GetValue(i);
                    result[i] = Convert.ToSingle(v, CultureInfo.InvariantCulture);
                }
                return result;
            }

            // IEnumerable（排除 string）
            if (obj is IEnumerable enumerable && obj is not string)
            {
                var list = new List<float>();
                foreach (var v in enumerable)
                {
                    list.Add(Convert.ToSingle(v, CultureInfo.InvariantCulture));
                }
                return list.ToArray();
            }

            // 单值数字
            if (obj is IConvertible)
            {
                return new[] { Convert.ToSingle(obj, CultureInfo.InvariantCulture) };
            }

            return Array.Empty<float>();
        }
        #endregion
    }
}