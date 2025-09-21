using System;
using System.Collections.Generic;
using UnityEngine;
using SaveFramework;
using SaveFramework.Runtime.Core;

namespace SaveFramework.Sample
{
    /// <summary>
    /// Sample component demonstrating the Save Framework usage
    /// </summary>
    public class PlayerStats : MonoBehaviour
    {
        [Header("Player Stats")]
        [Save("health")] public int Health = 100;
        [Save("mana")] public int Mana = 50;
        [Save("level")] public int Level = 1;
        [Save("exp")] public float Experience = 0f;

        [Header("Player Position")]
        [Save("pos")] public Vector3 Position;
        [Save("rot")] public Quaternion Rotation = Quaternion.identity;

        [Header("Player Inventory")]
        [Save] public List<int> Inventory = new List<int>();
        [Save("coins")] public int Currency = 0;

        [Header("Player Preferences")]
        [Save("volume")] public float MasterVolume = 1f;
        [Save("difficulty")] public GameDifficulty Difficulty = GameDifficulty.Normal;

        [Header("Advanced Data")]
        [Save("colors")] public Color[] FavoriteColors = new Color[] { Color.red, Color.blue, Color.green };
        [Save("waypoints")] public Vector3[] Waypoints = Array.Empty<Vector3>();

        [Header("Demo Controls")]
        [SerializeField] private string saveSlot = "demo_slot";
        [SerializeField] private bool logDebugInfo = true;

        private void Update()
        {
            // Demo save/load controls
            if (Input.GetKeyDown(KeyCode.F5))
            {
                Save();
            }

            if (Input.GetKeyDown(KeyCode.F9))
            {
                Load();
            }

            // Update position to current transform position
            Position = transform.position;
            Rotation = transform.rotation;
        }

        /// <summary>
        /// Save current stats to the demo slot
        /// </summary>
        [ContextMenu("保存统计数据")]
        public void Save()
        {
            try
            {
                SaveManager.Instance.Save(saveSlot);
                
                if (logDebugInfo)
                {
                    Debug.Log($"[PlayerStats] Saved to slot '{saveSlot}'\n" +
                             $"Health: {Health}, Mana: {Mana}, Level: {Level}\n" +
                             $"Position: {Position}, Rotation: {Rotation}\n" +
                             $"Inventory Items: {Inventory.Count}, Currency: {Currency}");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[PlayerStats] Failed to save: {ex.Message}");
            }
        }

        /// <summary>
        /// Load stats from the demo slot
        /// </summary>
        [ContextMenu("加载统计数据")]
        public void Load()
        {
            try
            {
                SaveManager.Instance.Load(saveSlot);
                
                // Apply loaded position to transform
                transform.position = Position;
                transform.rotation = Rotation;

                if (logDebugInfo)
                {
                    Debug.Log($"[PlayerStats] Loaded from slot '{saveSlot}'\n" +
                             $"Health: {Health}, Mana: {Mana}, Level: {Level}\n" +
                             $"Position: {Position}, Rotation: {Rotation}\n" +
                             $"Inventory Items: {Inventory.Count}, Currency: {Currency}");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[PlayerStats] 加载失败: {ex.Message}");
            }
        }

        /// <summary>
        /// Reset stats to default values
        /// </summary>
        [ContextMenu("重置统计数据")]
        public void ResetStats()
        {
            Health = 100;
            Mana = 50;
            Level = 1;
            Experience = 0f;
            Position = Vector3.zero;
            Rotation = Quaternion.identity;
            Inventory.Clear();
            Currency = 0;
            MasterVolume = 1f;
            Difficulty = GameDifficulty.Normal;
            FavoriteColors = new Color[] { Color.red, Color.blue, Color.green };
            Waypoints = new Vector3[0];

            transform.position = Position;
            transform.rotation = Rotation;

            if (logDebugInfo)
            {
                Debug.Log("[PlayerStats] Stats reset to default values");
            }
        }

        /// <summary>
        /// Add some demo items to inventory
        /// </summary>
        [ContextMenu("添加演示项目")]
        public void AddDemoItems()
        {
            Inventory.Add(1); // Sword
            Inventory.Add(2); // Shield
            Inventory.Add(3); // Potion
            Currency += 100;

            if (logDebugInfo)
            {
                Debug.Log("[PlayerStats] Added demo items to inventory");
            }
        }

        /// <summary>
        /// Level up the player
        /// </summary>
        [ContextMenu("升级")]
        public void LevelUp()
        {
            Level++;
            Health += 10;
            Mana += 5;
            Experience = 0f;

            if (logDebugInfo)
            {
                Debug.Log($"[PlayerStats] Leveled up to {Level}! Health: {Health}, Mana: {Mana}");
            }
        }

        private void OnGUI()
        {
            // Simple GUI for demo purposes
            var rect = new Rect(10, 10, 300, 500);
            GUILayout.BeginArea(rect);
            
            GUILayout.Label("Save Framework Demo", GUI.skin.box);
            GUILayout.Label($"Health: {Health} | Mana: {Mana} | Level: {Level}");
            GUILayout.Label($"Position: {Position:F1}");
            GUILayout.Label($"Items: {Inventory.Count} | Coins: {Currency}");
            GUILayout.Label($"Difficulty: {Difficulty}");
            
            GUILayout.Space(10);
            GUILayout.Label("Controls:");
            GUILayout.Label("F5 - Save | F9 - Load");
            
            if (GUILayout.Button("Reset Stats"))
                ResetStats();
            
            if (GUILayout.Button("Add Demo Items"))
                AddDemoItems();
                
            if (GUILayout.Button("Level Up"))
                LevelUp();
            
            GUILayout.EndArea();
        }
    }

    /// <summary>
    /// Example enum for difficulty settings
    /// </summary>
    public enum GameDifficulty
    {
        Easy,
        Normal,
        Hard,
        Nightmare
    }
}