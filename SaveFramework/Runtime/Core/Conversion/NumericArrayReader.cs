using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace SaveFramework.Runtime.Core.Conversion
{
    /// <summary>
    /// Utility for parsing numeric arrays from various JSON and object formats
    /// Handles JArray, JObject{x,y,z,w}/{r,g,b,a}, managed arrays, IEnumerable, and Unity types
    /// </summary>
    public static class NumericArrayReader
    {
        /// <summary>
        /// Parse a float array from various input formats
        /// </summary>
        public static float[] ReadFloatArray(object obj)
        {
            if (obj == null)
                return Array.Empty<float>();

            // Direct Unity types
            if (obj is Color c) return new[] { c.r, c.g, c.b, c.a };
            if (obj is Vector2 v2) return new[] { v2.x, v2.y };
            if (obj is Vector3 v3) return new[] { v3.x, v3.y, v3.z };
            if (obj is Vector4 v4) return new[] { v4.x, v4.y, v4.z, v4.w };
            if (obj is Quaternion q) return new[] { q.x, q.y, q.z, q.w };

            // JObject with named properties (e.g., {x: 1, y: 2, z: 3} or {r: 1, g: 0, b: 0, a: 1})
            if (obj is JObject jObj)
            {
                return ReadFromJObject(jObj);
            }

            // JArray
            if (obj is JArray jArray)
            {
                var result = new float[jArray.Count];
                for (int i = 0; i < jArray.Count; i++)
                {
                    result[i] = ConvertToFloat(jArray[i].ToObject<object>());
                }
                return result;
            }

            // Managed arrays (float[], double[], object[], etc.)
            if (obj is Array arr)
            {
                var len = arr.Length;
                var result = new float[len];
                for (int i = 0; i < len; i++)
                {
                    var value = arr.GetValue(i);
                    result[i] = ConvertToFloat(value);
                }
                return result;
            }

            // IEnumerable (excluding strings)
            if (obj is IEnumerable enumerable && obj is not string)
            {
                var list = new List<float>();
                foreach (var value in enumerable)
                {
                    list.Add(ConvertToFloat(value));
                }
                return list.ToArray();
            }

            // Single numeric value
            if (obj is IConvertible)
            {
                return new[] { ConvertToFloat(obj) };
            }

            return Array.Empty<float>();
        }

        /// <summary>
        /// Parse integer array from various input formats
        /// </summary>
        public static int[] ReadIntArray(object obj)
        {
            var floats = ReadFloatArray(obj);
            var result = new int[floats.Length];
            for (int i = 0; i < floats.Length; i++)
            {
                result[i] = Mathf.RoundToInt(floats[i]);
            }
            return result;
        }

        private static float[] ReadFromJObject(JObject jObj)
        {
            // Try Vector patterns first (x, y, z, w)
            if (TryReadVector(jObj, out var vector))
                return vector;

            // Try Color patterns (r, g, b, a)
            if (TryReadColor(jObj, out var color))
                return color;

            // Fallback: convert all numeric properties to array
            var list = new List<float>();
            foreach (var prop in jObj.Properties())
            {
                if (float.TryParse(prop.Value.ToString(), NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
                {
                    list.Add(value);
                }
            }
            return list.ToArray();
        }

        private static bool TryReadVector(JObject jObj, out float[] result)
        {
            result = null;

            var values = new List<float>();
            
            // Check for x, y, z, w pattern
            foreach (var component in new[] { "x", "y", "z", "w" })
            {
                if (jObj.TryGetValue(component, StringComparison.OrdinalIgnoreCase, out var token))
                {
                    values.Add(ConvertToFloat(token.ToObject<object>()));
                }
                else
                {
                    break; // Stop at first missing component
                }
            }

            if (values.Count >= 2) // At least Vector2
            {
                result = values.ToArray();
                return true;
            }

            return false;
        }

        private static bool TryReadColor(JObject jObj, out float[] result)
        {
            result = null;

            var values = new List<float>();
            
            // Check for r, g, b, a pattern
            foreach (var component in new[] { "r", "g", "b", "a" })
            {
                if (jObj.TryGetValue(component, StringComparison.OrdinalIgnoreCase, out var token))
                {
                    values.Add(ConvertToFloat(token.ToObject<object>()));
                }
                else if (component == "a" && values.Count == 3)
                {
                    // Alpha is optional, default to 1.0
                    values.Add(1.0f);
                    break;
                }
                else
                {
                    break; // Stop at first missing component
                }
            }

            if (values.Count >= 3) // At least RGB
            {
                result = values.ToArray();
                return true;
            }

            return false;
        }

        private static float ConvertToFloat(object value)
        {
            if (value == null)
                return 0f;

            try
            {
                return Convert.ToSingle(value, CultureInfo.InvariantCulture);
            }
            catch
            {
                return 0f;
            }
        }
    }
}