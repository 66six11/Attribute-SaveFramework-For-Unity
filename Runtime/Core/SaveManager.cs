using System;
using System.Collections.Generic;
using System.Linq;
using SaveFramework.Components;
using UnityEngine;

namespace SaveFramework.Runtime.Core
{
    /// <summary>
    /// Main manager for the Save Framework
    /// </summary>
    public class SaveManager : MonoBehaviour
    {
        private static SaveManager instance;
        public static SaveManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<SaveManager>();
                    if (instance == null)
                    {
                        var go = new GameObject("SaveManager");
                        instance = go.AddComponent<SaveManager>();
                        DontDestroyOnLoad(go);
                    }
                }
                return instance;
            }
        }

        [SerializeField] private ISaveBackend backend;
        private readonly Dictionary<Type, Dictionary<string, SaveEntry>> cachedSaveEntries = new Dictionary<Type, Dictionary<string, SaveEntry>>();

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeBackend();
            }
            else if (instance != this)
            {
                Destroy(gameObject);
            }
        }

        private void InitializeBackend()
        {
            if (backend == null)
            {
                backend = new FileBackend();
            }
        }

        /// <summary>
        /// Set a custom save backend
        /// </summary>
        public void SetBackend(ISaveBackend customBackend)
        {
            backend = customBackend ?? throw new ArgumentNullException(nameof(customBackend));
        }

        /// <summary>
        /// Save all SaveId components to the specified slot
        /// </summary>
        public void Save(string slotName)
        {
            if (string.IsNullOrEmpty(slotName))
                throw new ArgumentException("Slot name cannot be null or empty", nameof(slotName));

            try
            {
                var saveData = new SaveData();
                var saveIds = FindObjectsOfType<SaveId>();

                Debug.Log($"Starting save to slot '{slotName}' with {saveIds.Length} SaveId components");

                foreach (var saveId in saveIds)
                {
                    if (saveId == null || string.IsNullOrEmpty(saveId.Id))
                        continue;

                    SaveComponent(saveId, saveData);
                }

                backend.Save(slotName, saveData.Data);
                Debug.Log($"Save completed successfully to slot '{slotName}' with {saveData.Count} entries");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to save to slot '{slotName}': {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Load data from the specified slot and apply to all SaveId components
        /// </summary>
        public void Load(string slotName)
        {
            if (string.IsNullOrEmpty(slotName))
                throw new ArgumentException("Slot name cannot be null or empty", nameof(slotName));

            try
            {
                var data = backend.Load(slotName);
                var saveData = new SaveData(data);
                var saveIds = FindObjectsOfType<SaveId>();

                Debug.Log($"Starting load from slot '{slotName}' with {saveData.Count} entries for {saveIds.Length} SaveId components");

                foreach (var saveId in saveIds)
                {
                    if (saveId == null || string.IsNullOrEmpty(saveId.Id))
                        continue;

                    LoadComponent(saveId, saveData);
                }

                Debug.Log($"Load completed successfully from slot '{slotName}'");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to load from slot '{slotName}': {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Check if a save slot exists
        /// </summary>
        public bool HasSave(string slotName)
        {
            return backend?.HasSave(slotName) ?? false;
        }

        /// <summary>
        /// Delete a save slot
        /// </summary>
        public void DeleteSave(string slotName)
        {
            backend?.DeleteSave(slotName);
        }

        /// <summary>
        /// Get all available save slot names
        /// </summary>
        public string[] GetSaveSlots()
        {
            return backend?.GetSaveSlots() ?? new string[0];
        }

        private void SaveComponent(SaveId saveId, SaveData saveData)
        {
            var components = saveId.GetComponents<MonoBehaviour>();

            foreach (var component in components)
            {
                if (component == null)
                    continue;

                var componentType = component.GetType();
                var saveEntries = GetSaveEntries(componentType);

                if (saveEntries.Count == 0)
                    continue;

                foreach (var kvp in saveEntries)
                {
                    var entry = kvp.Value;
                    var fullKey = $"{saveId.Id}.{componentType.Name}.{entry.Key}";

                    try
                    {
                        var value = entry.GetValue(component);
                        var jsonValue = Converters.ToJsonValue(value, entry.FieldType);
                        saveData.SetValue(fullKey, jsonValue);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"Failed to save field '{entry.FieldName}' in {componentType.Name}: {ex.Message}");
                    }
                }
            }
        }

        private void LoadComponent(SaveId saveId, SaveData saveData)
        {
            var components = saveId.GetComponents<MonoBehaviour>();

            foreach (var component in components)
            {
                if (component == null)
                    continue;

                var componentType = component.GetType();
                var saveEntries = GetSaveEntries(componentType);

                if (saveEntries.Count == 0)
                    continue;

                foreach (var kvp in saveEntries)
                {
                    var entry = kvp.Value;
                    var fullKey = $"{saveId.Id}.{componentType.Name}.{entry.Key}";

                    // Try the main key first, then try aliases
                    object jsonValue = null;
                    if (saveData.HasKey(fullKey))
                    {
                        jsonValue = saveData.GetValue(fullKey, entry.FieldType);
                    }
                    else
                    {
                        // Try aliases
                        foreach (var alias in entry.Aliases)
                        {
                            var aliasKey = $"{saveId.Id}.{componentType.Name}.{alias}";
                            if (saveData.HasKey(aliasKey))
                            {
                                jsonValue = saveData.GetValue(aliasKey, entry.FieldType);
                                break;
                            }
                        }
                    }

                    if (jsonValue != null)
                    {
                        try
                        {
                            var convertedValue = Converters.FromJsonValue(jsonValue, entry.FieldType);
                            entry.SetValue(component, convertedValue);
                        }
                        catch (Exception ex)
                        {
                            Debug.LogError($"Failed to load field '{entry.FieldName}' in {componentType.Name}: {ex.Message}");
                        }
                    }
                }
            }
        }

        private Dictionary<string, SaveEntry> GetSaveEntries(Type componentType)
        {
            if (cachedSaveEntries.TryGetValue(componentType, out var cached))
                return cached;

            var entries = new Dictionary<string, SaveEntry>();

            // First try to get from generated registry
            if (SaveRegistryManager.HasRegistryFor(componentType))
            {
                entries = SaveRegistryManager.GetSaveEntries(componentType);
            }
            else
            {
                // Fallback to reflection-based discovery
                entries = DiscoverSaveEntriesReflection(componentType);
            }

            cachedSaveEntries[componentType] = entries;
            return entries;
        }

        private Dictionary<string, SaveEntry> DiscoverSaveEntriesReflection(Type componentType)
        {
            var entries = new Dictionary<string, SaveEntry>();
            var fields = componentType.GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            foreach (var field in fields)
            {
                var saveAttr = field.GetCustomAttributes(typeof(SaveAttribute), true).FirstOrDefault() as SaveAttribute;
                if (saveAttr == null)
                {
                    // Also check for SaveFieldAttribute
                    saveAttr = field.GetCustomAttributes(typeof(SaveFieldAttribute), true).FirstOrDefault() as SaveFieldAttribute;
                }

                if (saveAttr == null)
                    continue;

                if (!Converters.IsSupported(field.FieldType))
                {
                    Debug.LogWarning($"Field '{field.Name}' in {componentType.Name} has unsupported type '{field.FieldType.Name}' and will be skipped.");
                    continue;
                }

                var key = saveAttr.EffectiveKey(field.Name);
                var entry = new SaveEntry(key, saveAttr.Aliases, componentType, field.Name, field.FieldType, field);

                entries[key] = entry;
            }

            return entries;
        }
    }
}