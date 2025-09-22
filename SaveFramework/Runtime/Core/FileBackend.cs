using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

namespace SaveFramework.Runtime.Core
{
    /// <summary>
    /// 使用 JSON 格式的基于文件的保存后端
    /// </summary>
    public class FileBackend : ISaveBackend
    {
        private readonly string savesDirectory;

        public FileBackend()
        {
            savesDirectory = Path.Combine(Application.persistentDataPath, "Saves");

            // Ensure the saves directory exists
            if (!Directory.Exists(savesDirectory))
            {
                Directory.CreateDirectory(savesDirectory);
            }
        }

        public FileBackend(string customDirectory)
        {
            savesDirectory = customDirectory;

            if (!Directory.Exists(savesDirectory))
            {
                Directory.CreateDirectory(savesDirectory);
            }
        }

        public void Save(string slotName, Dictionary<string, object> data, bool reWrite = true)
        {
            if (string.IsNullOrEmpty(slotName))
                throw new ArgumentException("插槽名称不能为空或空", nameof(slotName));

            try
            {
                var filePath = GetSaveFilePath(slotName);
                var json = JsonConvert.SerializeObject(data, Formatting.Indented);
                if (reWrite)
                {
                    File.WriteAllText(filePath, json);
                }
                else
                {
                    // 1) 读旧数据
                    Dictionary<string, object> finalData;
                    if (File.Exists(filePath))
                    {
                        var oldJson = File.ReadAllText(filePath);
                        finalData = string.IsNullOrWhiteSpace(oldJson)
                            ? new Dictionary<string, object>()
                            : Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(oldJson)
                              ?? new Dictionary<string, object>();
                    }
                    else
                    {
                        finalData = new Dictionary<string, object>();
                    }

                    // 2) 合并补丁（有则覆盖，无则新增）
                    foreach (var kv in data)
                        finalData[kv.Key] = kv.Value;
                    // 3) 原子写入（避免中途崩溃导致文件损坏）
                    var tmp = filePath + ".tmp";
                    File.WriteAllText(tmp, json);
                    // 用覆盖移动替代直接写，尽量保证原子性（注意：不同平台对 Replace/Move 语义稍有差异）
                    if (File.Exists(filePath))
                    {
                        // Windows 下可用 File.Replace；跨平台使用先删再改名
                        File.Delete(filePath);
                    }
                    File.Move(tmp, filePath);
                }

                Debug.Log($"保存完成： {filePath}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"无法保存到插槽'{slotName}': {ex.Message}");
                throw;
            }
        }

        public Dictionary<string, object> Load(string slotName)
        {
            if (string.IsNullOrEmpty(slotName))
                throw new ArgumentException("插槽名称不能为空或空", nameof(slotName));

            try
            {
                var filePath = GetSaveFilePath(slotName);

                if (!File.Exists(filePath))
                {
                    Debug.LogWarning($"未找到保存文件： {filePath}");
                    return new Dictionary<string, object>();
                }

                var json = File.ReadAllText(filePath);
                var data = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);

                Debug.Log($"加载完成：{filePath}");
                return data ?? new Dictionary<string, object>();
            }
            catch (Exception ex)
            {
                Debug.LogError($"无法从插槽加载'{slotName}': {ex.Message}");
                return new Dictionary<string, object>();
            }
        }

        public bool HasSave(string slotName)
        {
            if (string.IsNullOrEmpty(slotName))
                return false;

            return File.Exists(GetSaveFilePath(slotName));
        }

        public void DeleteSave(string slotName)
        {
            if (string.IsNullOrEmpty(slotName))
                throw new ArgumentException("插槽名称不能为空或空", nameof(slotName));

            try
            {
                var filePath = GetSaveFilePath(slotName);

                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    Debug.Log($"保存已删除： {filePath}");
                }
                else
                {
                    Debug.LogWarning($"未找到保存文件以进行删除： {filePath}");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"无法删除保存槽'{slotName}': {ex.Message}");
                throw;
            }
        }

        public string[] GetSaveSlots()
        {
            try
            {
                if (!Directory.Exists(savesDirectory))
                    return Array.Empty<string>();

                var files = Directory.GetFiles(savesDirectory, "*.json");
                var slots = new string[files.Length];

                for (int i = 0; i < files.Length; i++)
                {
                    slots[i] = Path.GetFileNameWithoutExtension(files[i]);
                }

                return slots;
            }
            catch (Exception ex)
            {
                Debug.LogError($"无法获取保存槽： {ex.Message}");
                return Array.Empty<string>();
            }
        }

        private string GetSaveFilePath(string slotName)
        {
            // Sanitize the slot name to prevent path traversal
            var sanitizedName = string.Empty;
            foreach (char c in slotName)
            {
                if (char.IsLetterOrDigit(c) || c == '_' || c == '-')
                    sanitizedName += c;
                else
                    sanitizedName += '_';
            }

            return Path.Combine(savesDirectory, $"{sanitizedName}.json");
        }
        
    }
}