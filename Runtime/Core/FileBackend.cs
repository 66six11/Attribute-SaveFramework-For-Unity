using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

namespace SaveFramework.Runtime.Core
{
    /// <summary>
    /// File-based save backend using JSON format
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

        public void Save(string slotName, Dictionary<string, object> data)
        {
            if (string.IsNullOrEmpty(slotName))
                throw new ArgumentException("Slot name cannot be null or empty", nameof(slotName));

            try
            {
                var filePath = GetSaveFilePath(slotName);
                var json = JsonConvert.SerializeObject(data, Formatting.Indented);
                File.WriteAllText(filePath, json);
                
                Debug.Log($"Save completed: {filePath}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to save to slot '{slotName}': {ex.Message}");
                throw;
            }
        }

        public Dictionary<string, object> Load(string slotName)
        {
            if (string.IsNullOrEmpty(slotName))
                throw new ArgumentException("Slot name cannot be null or empty", nameof(slotName));

            try
            {
                var filePath = GetSaveFilePath(slotName);
                
                if (!File.Exists(filePath))
                {
                    Debug.LogWarning($"Save file not found: {filePath}");
                    return new Dictionary<string, object>();
                }

                var json = File.ReadAllText(filePath);
                var data = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
                
                Debug.Log($"Load completed: {filePath}");
                return data ?? new Dictionary<string, object>();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to load from slot '{slotName}': {ex.Message}");
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
                throw new ArgumentException("Slot name cannot be null or empty", nameof(slotName));

            try
            {
                var filePath = GetSaveFilePath(slotName);
                
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    Debug.Log($"Save deleted: {filePath}");
                }
                else
                {
                    Debug.LogWarning($"Save file not found for deletion: {filePath}");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to delete save slot '{slotName}': {ex.Message}");
                throw;
            }
        }

        public string[] GetSaveSlots()
        {
            try
            {
                if (!Directory.Exists(savesDirectory))
                    return new string[0];

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
                Debug.LogError($"Failed to get save slots: {ex.Message}");
                return new string[0];
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