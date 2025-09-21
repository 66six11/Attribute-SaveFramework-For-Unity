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
        /// Initialize the converter registry by discovering and registering all auto-registered converters
        /// </summary>
        public static void Initialize()
        {
            if (isInitialized)
                return;

            try
            {
                var assemblies = AppDomain.CurrentDomain.GetAssemblies();

                foreach (var assembly in assemblies)
                {
                    try
                    {
                        var converterTypes = assembly.GetTypes()
                                                     .Where(type => type.GetCustomAttribute<AutoRegisterConverterAttribute>() != null)
                                                     .Where(type => typeof(IValueConverter).IsAssignableFrom(type))
                                                     .Where(type => !type.IsAbstract && !type.IsInterface);

                        foreach (var converterType in converterTypes)
                        {
                            try
                            {
                                var instance = Activator.CreateInstance(converterType) as IValueConverter;
                                if (instance != null)
                                {
                                    RegisterConverter(instance);
                                }
                            }
                            catch (Exception ex)
                            {
                                Debug.LogWarning($"Failed to create converter instance {converterType.Name}: {ex.Message}");
                            }
                        }
                    }
                    catch (ReflectionTypeLoadException ex)
                    {
                        // Skip assemblies that can't be loaded
                        Debug.LogWarning($"Failed to load types from assembly {assembly.FullName}: {ex.Message}");
                    }
                }

                isInitialized = true;
                Debug.Log($"ConverterRegistry initialized with {converters.Count} converters");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to initialize ConverterRegistry: {ex.Message}");
            }
        }

        /// <summary>
        /// Manually register a converter
        /// </summary>
        public static void RegisterConverter(IValueConverter converter)
        {
            if (converter == null)
                return;

            converters[converter.TargetType] = converter;
        }

        /// <summary>
        /// Get a converter for the specified type
        /// </summary>
        public static IValueConverter GetConverter(Type type)
        {
            EnsureInitialized();
            return converters.TryGetValue(type, out var converter) ? converter : null;
        }

        /// <summary>
        /// 检查是否存在指定类型的转换器
        /// </summary>
        public static bool HasConverter(Type type)
        {
            EnsureInitialized();
            return converters.ContainsKey(type);
        }

        /// <summary>
        /// 获取所有注册的转换器类型
        /// </summary>
        public static Type[] GetSupportedTypes()
        {
            EnsureInitialized();
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

        private static void EnsureInitialized()
        {
            if (!isInitialized)
                Initialize();
        }

        /// <summary>
        /// Initialize the registry on Unity startup
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void InitializeOnStartup()
        {
            Initialize();
        }
    }
}