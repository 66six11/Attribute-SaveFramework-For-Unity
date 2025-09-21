using System;
using System.Collections;
using System.Collections.Generic;
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

            // Handle enums
            if (targetType.IsEnum)
                return Enum.Parse(targetType, jsonValue.ToString());

            // Handle arrays
            if (targetType.IsArray)
            {
                var elementType = targetType.GetElementType();
                var jsonArray = jsonValue as JArray ?? new JArray(jsonValue);
                var array = Array.CreateInstance(elementType, jsonArray.Count);
                
                for (int i = 0; i < jsonArray.Count; i++)
                {
                    array.SetValue(FromJsonValue(jsonArray[i], elementType), i);
                }
                
                return array;
            }

            // Handle Lists
            if (targetType.IsGenericType && targetType.GetGenericTypeDefinition() == typeof(List<>))
            {
                var elementType = targetType.GetGenericArguments()[0];
                var jsonArray = jsonValue as JArray ?? new JArray(jsonValue);
                var list = (IList)Activator.CreateInstance(targetType);
                
                foreach (var item in jsonArray)
                {
                    list.Add(FromJsonValue(item, elementType));
                }
                
                return list;
            }

            // Handle custom converters
            if (FromJsonConverters.TryGetValue(targetType, out var converter))
                return converter(jsonValue);

            // Handle primitive type conversion
            try
            {
                return Convert.ChangeType(jsonValue, targetType);
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

        private static float[] GetFloatArray(object obj)
        {
            if (obj is JArray jArray)
            {
                var result = new float[jArray.Count];
                for (int i = 0; i < jArray.Count; i++)
                {
                    result[i] = jArray[i].Value<float>();
                }
                return result;
            }

            if (obj is float[] floatArray)
                return floatArray;

            return new float[0];
        }
        #endregion
    }
}