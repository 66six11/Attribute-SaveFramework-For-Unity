using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace SaveFramework.Runtime.Core.Conversion
{
    /// <summary>
    /// Attribute to mark converters for automatic registration
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class AutoRegisterConverterAttribute : Attribute
    {
    }

    /// <summary>
    /// Registry for value converters with automatic discovery and registration
    /// </summary>
    public static class ConverterRegistry
    {
        private static readonly Dictionary<Type, IValueConverter> converters = new Dictionary<Type, IValueConverter>();
        private static bool isInitialized = false;

        /// <summary>
        /// 通过发现和注册所有自动注册的转换器来初始化转换器注册表
        /// </summary>
       

        /// <summary>
        /// Manually register a converter
        /// </summary>
        public static void RegisterConverter(IValueConverter converter)
        {
            if (converter == null)
                return;
            if (!converters.TryAdd(converter.TargetType, converter))
                return;
        }

        /// <summary>
        /// 获取指定类型的转换器
        /// </summary>
        public static IValueConverter GetConverter(Type type)
        {
          
            return converters.TryGetValue(type, out var converter) ? converter : null;
        }

        /// <summary>
        /// 检查是否存在指定类型的转换器
        /// </summary>
        public static bool HasConverter(Type type)
        {
          
            return converters.ContainsKey(type);
        }

        /// <summary>
        /// 获取所有注册的转换器类型
        /// </summary>
        public static Type[] GetSupportedTypes()
        {
            
            return converters.Keys.ToArray();
        }

        /// <summary>
        ///清除所有已注册的转换器（用于测试）
        /// </summary>
        public static void ClearConverters()
        {
            converters.Clear();
            isInitialized = false;
        }

        public static bool IsSupported(Type type)
        {
            if (type == null) return false;

            if (type.IsPrimitive || type == typeof(string) || type.IsEnum)
                return true;

            if (HasConverter(type))
                return true;

            if (type.IsArray)
                return IsSupported(type.GetElementType());

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
                return IsSupported(type.GetGenericArguments()[0]);

            return false;
        }

      
    }
}