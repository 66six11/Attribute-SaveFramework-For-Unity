using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SaveFramework.Components;
using SaveFramework.Runtime.Attributes;
using UnityEngine;

namespace SaveFramework.Runtime.Core
{
    /// <summary>
    /// 保存框架的主管理器
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
                    instance = FindFirstObjectByType<SaveManager>();
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
        /// 设置自定义保存后端
        /// </summary>
        public void SetBackend(ISaveBackend customBackend)
        {
            backend = customBackend ?? throw new ArgumentNullException(nameof(customBackend));
        }

        /// <summary>
        /// 将所有 SaveId 组件保存到指定的插槽中
        /// </summary>
        public void Save(string slotName)
        {
            if (string.IsNullOrEmpty(slotName))
                throw new ArgumentException("插槽名称不能为空或空", nameof(slotName));

            try
            {
                var saveData = new SaveData();
                var saveIds = FindObjectsByType<SaveId>(FindObjectsSortMode.None);

                Debug.Log($"开始保存到插槽'{slotName}' 使用 {saveIds.Length} SaveId 组件");

                foreach (var saveId in saveIds)
                {
                    if (saveId == null || string.IsNullOrEmpty(saveId.Id))
                        continue;

                    SaveComponent(saveId, saveData);
                }

                backend.Save(slotName, saveData.Data);
                Debug.Log($"已成功完成保存到插槽 '{slotName}' ： {saveData.Count} 条目");
            }
            catch (Exception ex)
            {
                Debug.LogError($"无法保存到插槽 '{slotName}': {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 从指定的插槽加载数据并应用于所有 SaveId 组件
        /// </summary>
        public void Load(string slotName)
        {
            if (string.IsNullOrEmpty(slotName))
                throw new ArgumentException("插槽名称不能为空或空", nameof(slotName));

            try
            {
                var data = backend.Load(slotName);
                var saveData = new SaveData(data);
                var saveIds = FindObjectsByType<SaveId>(FindObjectsSortMode.None);

                Debug.Log($"从 slot '{slotName}' 开始加载，为 {saveIds.Length} SaveId 组件提供 {saveData.Count} 条目");
                //打印数据
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("加载数据：");
                foreach (var kvp in saveData.Data)
                {
                    sb.AppendLine(kvp.Key + " : " + kvp.Value);
                }
                Debug.Log(sb.ToString());
                foreach (var saveId in saveIds)
                {
                    if (saveId == null || string.IsNullOrEmpty(saveId.Id))
                    {
                        Debug.Log($"SaveId 组件 '{saveId.name}' 未设置 ID，无法加载");
                        continue; 
                    }
                       

                    LoadComponent(saveId, saveData);
                }

                Debug.Log($"从插槽成功完成加载 '{slotName}'");
            }
            catch (Exception ex)
            {
                Debug.LogError($"无法从插槽加载 '{slotName}': {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 检查是否存在保存槽
        /// </summary>
        public bool HasSave(string slotName)
        {
            return backend?.HasSave(slotName) ?? false;
        }

        /// <summary>
        /// 删除保存槽
        /// </summary>
        public void DeleteSave(string slotName)
        {
            backend?.DeleteSave(slotName);
        }

        /// <summary>
        /// 获取所有可用的保存槽名称
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
                        Debug.LogError($"无法保存字段 '{entry.FieldName}' in {componentType.Name}: {ex.Message}");
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
                    object valueObj = null;
                    if (saveData.HasKey(fullKey))
                    {
                        valueObj = saveData.GetValue(fullKey, entry.FieldType);
                    }
                    else
                    {
                        // Try aliases
                        foreach (var alias in entry.Aliases)
                        {
                            var aliasKey = $"{saveId.Id}.{componentType.Name}.{alias}";
                            if (saveData.HasKey(aliasKey))
                            {
                                valueObj = saveData.GetValue(aliasKey, entry.FieldType);
                                break;
                            }
                        }
                    }

                    if (valueObj != null)
                    {
                        try
                        {
                            // Direct assignment if value is already the correct type (avoid double-conversion)
                            if (entry.FieldType.IsInstanceOfType(valueObj))
                            {
                                Debug.Log($"直接加载字段 '{entry.FieldName}' in {componentType.Name} : {valueObj}");
                                entry.SetValue(component, valueObj);
                            }
                            else
                            {
                                var convertedValue = Converters.FromJsonValue(valueObj, entry.FieldType);
                                Debug.Log($"转换加载字段 '{entry.FieldName}' in {componentType.Name} : {convertedValue}");
                                entry.SetValue(component, convertedValue);
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.LogError($"加载失败字段 '{entry.FieldName}' in {componentType.Name}: {ex.Message}");
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

            // 首先尝试从生成的注册表中获取
            if (SaveRegistryManager.HasRegistryFor(componentType))
            {
                entries = SaveRegistryManager.GetSaveEntries(componentType);
            }
            else
            {
                // 回退到基于反射的发现
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
                    continue;

                if (!Converters.IsSupported(field.FieldType))
                {
                    Debug.LogWarning($"字段 '{field.Name}' 在 {componentType.Name} 是不受支持的类型 '{field.FieldType.Name}' 将被跳过。");
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