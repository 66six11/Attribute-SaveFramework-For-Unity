using System;
using System.Collections.Generic;
using UnityEngine;

namespace SaveFramework.Runtime.Core
{
    /// <summary>
    /// 生成的保存注册表的基类
    /// </summary>
    public abstract class GeneratedSaveRegistry
    {
        /// <summary>
        /// 注册给定组件类型生成的所有保存条目
        /// </summary>
        public abstract void RegisterSaveEntries(Type componentType, Dictionary<string, SaveEntry> entries);

        /// <summary>
        /// 检查此注册表是否处理给定的组件类型
        /// </summary>
        public abstract bool HandlesType(Type componentType);

        /// <summary>
        /// 获取此注册表处理的所有类型
        /// </summary>
        public abstract Type[] GetHandledTypes();
    }

    /// <summary>
    /// 生成的保存注册表的管理器
    /// </summary>
    public static class SaveRegistryManager
    {
        private static readonly List<GeneratedSaveRegistry> registries = new List<GeneratedSaveRegistry>();
        private static bool isInitialized = false;

        /// <summary>
        /// Register a generated save registry
        /// </summary>
        public static void RegisterRegistry(GeneratedSaveRegistry registry)
        {
            if (registry == null)
                return;

            if (!registries.Contains(registry))
            {
                registries.Add(registry);
                Debug.Log($"已注册的保存注册表: {registry.GetType().Name}");
            }
        }

        /// <summary>
        /// 从已注册的注册表中获取组件类型的保存条目
        /// </summary>
        public static Dictionary<string, SaveEntry> GetSaveEntries(Type componentType)
        {
            EnsureInitialized();

            var entries = new Dictionary<string, SaveEntry>();

            foreach (var registry in registries)
            {
                if (registry.HandlesType(componentType))
                {
                    registry.RegisterSaveEntries(componentType, entries);
                    break; // 仅使用处理此类型的第一个注册表
                }
            }

            return entries;
        }

        /// <summary>
        /// 检查是否有任何注册表处理给定类型
        /// </summary>
        public static bool HasRegistryFor(Type componentType)
        {
            EnsureInitialized();

            foreach (var registry in registries)
            {
                if (registry.HandlesType(componentType))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// 获取所有已注册的注册表
        /// </summary>
        public static GeneratedSaveRegistry[] GetRegistries()
        {
            EnsureInitialized();
            return registries.ToArray();
        }

        /// <summary>
        /// 清除所有注册注册表（用于测试）
        /// </summary>
        public static void ClearRegistries()
        {
            registries.Clear();
            isInitialized = false;
        }

        private static void EnsureInitialized()
        {
            if (isInitialized)
                return;

            // 尝试查找并初始化生成的注册表
            TryInitializeGeneratedRegistry();
            isInitialized = true;
        }

        private static void TryInitializeGeneratedRegistry()
        {
            try
            {
                // 查找生成的注册表类
                var generatedType = Type.GetType("SaveFramework.Generated.SaveFrameworkRegistry");
                if (generatedType != null)
                {
                    if (Activator.CreateInstance(generatedType) is GeneratedSaveRegistry instance)
                    {
                        RegisterRegistry(instance);
                        Debug.Log("自动注册生成的保存注册表");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"无法自动初始化生成的注册表: {ex.Message}");
            }
        }
    }
}