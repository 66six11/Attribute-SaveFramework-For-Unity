using System;
using System.Collections.Generic;
using UnityEngine;

namespace SaveFramework.Runtime.Core
{
    /// <summary>
    /// Base class for generated save registries
    /// </summary>
    public abstract class GeneratedSaveRegistry
    {
        /// <summary>
        /// Register all generated save entries for the given component type
        /// </summary>
        public abstract void RegisterSaveEntries(Type componentType, Dictionary<string, SaveEntry> entries);

        /// <summary>
        /// Check if this registry handles the given component type
        /// </summary>
        public abstract bool HandlesType(Type componentType);

        /// <summary>
        /// Get all types handled by this registry
        /// </summary>
        public abstract Type[] GetHandledTypes();
    }

    /// <summary>
    /// Manager for generated save registries
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
                Debug.Log($"Registered save registry: {registry.GetType().Name}");
            }
        }

        /// <summary>
        /// Get save entries for a component type from registered registries
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
                    break; // Only use the first registry that handles this type
                }
            }

            return entries;
        }

        /// <summary>
        /// Check if any registry handles the given type
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
        /// Get all registered registries
        /// </summary>
        public static GeneratedSaveRegistry[] GetRegistries()
        {
            EnsureInitialized();
            return registries.ToArray();
        }

        /// <summary>
        /// Clear all registered registries (used for testing)
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

            // Try to find and initialize the generated registry
            TryInitializeGeneratedRegistry();
            isInitialized = true;
        }

        private static void TryInitializeGeneratedRegistry()
        {
            try
            {
                // Look for the generated registry class
                var generatedType = Type.GetType("SaveFramework.Generated.SaveFrameworkRegistry");
                if (generatedType != null)
                {
                    var instance = Activator.CreateInstance(generatedType) as GeneratedSaveRegistry;
                    if (instance != null)
                    {
                        RegisterRegistry(instance);
                        Debug.Log("Automatically registered generated save registry");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Failed to auto-initialize generated registry: {ex.Message}");
            }
        }
    }
}