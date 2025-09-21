# SaveFramework User Guide

## Overview

SaveFramework is a lightweight, easy-to-use save system framework designed specifically for Unity. It enables automatic save and load of game data through simple attribute marking, supports multiple data types, and provides flexible extension mechanisms.

## Key Features

- ✅ **Easy to Use**: Just add `[Save]` attribute to mark fields for saving
- ✅ **Auto Serialization**: Automatically handles Unity common types (Vector3, Quaternion, Color, etc.)
- ✅ **Multiple Save Slots**: Support for multiple independent save slots
- ✅ **Type Safe**: Compile-time checking to avoid runtime errors
- ✅ **Highly Extensible**: Support for custom type converters
- ✅ **Performance Optimized**: Uses code generation to avoid reflection overhead
- ✅ **Cross Platform**: Supports all Unity target platforms

## Quick Start

### 1. Installation

Place the SaveFramework folder in your Unity project's Assets directory.

### 2. Basic Setup

#### Add SaveId Component
Add the `SaveId` component to GameObjects that need to save data:

```csharp
using UnityEngine;
using SaveFramework.Components;

public class Player : MonoBehaviour
{
    // SaveId component will automatically add unique identifier
}
```

#### Mark Fields for Saving
Use the `[Save]` attribute to mark fields that need to be saved:

```csharp
using SaveFramework.Runtime.Attributes;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Save("health")] public int Health = 100;
    [Save("mana")] public int Mana = 50;
    [Save("level")] public int Level = 1;
    [Save] public Vector3 Position;  // Use field name as key
}
```

#### Generate Registration Code
In Unity Editor, select: **Tools → Save Framework → Generate Registration**

This will scan all fields with `[Save]` attribute and generate necessary registration code.

### 3. Save and Load Data

```csharp
using SaveFramework.Runtime.Core;

// Save data to specified slot
SaveManager.Instance.Save("player_save");

// Load data from specified slot
SaveManager.Instance.Load("player_save");

// Check if save exists
bool hasSave = SaveManager.Instance.HasSave("player_save");

// Delete save
SaveManager.Instance.DeleteSave("player_save");
```

## Detailed Usage Guide

### Save Attribute Details

#### Basic Usage

```csharp
public class GameData : MonoBehaviour
{
    // Use custom key name
    [Save("player_hp")] public int Health = 100;
    
    // Use field name as key (recommended for simple cases)
    [Save] public int Mana = 50;
    
    // Support aliases (for data migration)
    [Save("exp", "experience", "xp")] public float Experience = 0f;
}
```

#### Supported Data Types

**Basic Types:**
```csharp
[Save] public int IntValue;
[Save] public float FloatValue;
[Save] public string StringValue;
[Save] public bool BoolValue;
[Save] public char CharValue;
[Save] public byte ByteValue;
[Save] public long LongValue;
[Save] public uint UIntValue;
[Save] public ulong ULongValue;
[Save] public sbyte SByteValue;
```

**Unity Types:**
```csharp
[Save] public Vector2 Position2D;
[Save] public Vector3 Position3D;
[Save] public Vector4 Position4D;
[Save] public Quaternion Rotation;
[Save] public Color PlayerColor;
```

**Arrays and Collections:**
```csharp
[Save] public int[] IntArray;
[Save] public Vector3[] Waypoints;
[Save] public List<string> ItemNames;
[Save] public Color[] FavoriteColors;
```

**Enum Types:**
```csharp
public enum Difficulty { Easy, Normal, Hard }

[Save] public Difficulty GameDifficulty;
```

### SaveId Component Details

The `SaveId` component provides a unique identifier for each GameObject and is the core component of the save system.

#### Auto-Generated ID
```csharp
// SaveId will automatically generate unique ID when component is added
var saveId = gameObject.AddComponent<SaveId>();
Debug.Log(saveId.Id); // Output like: "a1b2c3d4-e5f6-7890-abcd-ef1234567890"
```

#### Custom ID
```csharp
var saveId = GetComponent<SaveId>();
saveId.SetCustomId("player_main"); // Set predictable ID
```

#### Utility Methods
```csharp
var saveId = GetComponent<SaveId>();

// Check if has valid ID
if (saveId.HasValidId())
{
    Debug.Log("SaveId is set");
}

// Get display name (for debugging)
Debug.Log(saveId.GetDisplayName()); // Output: "Player (a1b2c3d4...)"

// Regenerate ID
saveId.GenerateNewId();
```

### Save Backend Configuration

#### Default File Backend
The framework uses file system storage by default, located at `Application.persistentDataPath/Saves/`.

#### Custom Save Directory
```csharp
using SaveFramework.Runtime.Core;

public class CustomSaveSetup : MonoBehaviour
{
    void Awake()
    {
        // Set custom save directory
        var customBackend = new FileBackend("/custom/save/path");
        SaveManager.Instance.SetBackend(customBackend);
    }
}
```

#### Implement Custom Backend
```csharp
using SaveFramework.Runtime.Core;
using System.Collections.Generic;

public class CloudSaveBackend : ISaveBackend
{
    public void Save(string slotName, Dictionary<string, object> data)
    {
        // Implement cloud save logic
    }
    
    public Dictionary<string, object> Load(string slotName)
    {
        // Implement cloud load logic
        return new Dictionary<string, object>();
    }
    
    public bool HasSave(string slotName)
    {
        // Check if cloud save exists
        return false;
    }
    
    public void DeleteSave(string slotName)
    {
        // Delete cloud save
    }
    
    public string[] GetSaveSlots()
    {
        // Get all cloud save slots
        return new string[0];
    }
}
```

### Advanced Features

#### Custom Type Converters

For types not natively supported by the framework, you can implement custom converters:

```csharp
using SaveFramework.Runtime.Core.Conversion;

[AutoRegisterConverter]
public class DateTimeConverter : IValueConverter
{
    public bool CanConvert(Type type)
    {
        return type == typeof(DateTime);
    }
    
    public object ToJsonValue(object value)
    {
        return ((DateTime)value).ToBinary();
    }
    
    public object FromJsonValue(object jsonValue, Type targetType)
    {
        return DateTime.FromBinary((long)jsonValue);
    }
}
```

#### Field Marking with Converter
```csharp
public class GameSession : MonoBehaviour
{
    [Save("start_time", typeof(DateTimeConverter))] 
    public DateTime StartTime = DateTime.Now;
}
```

## Complete Example

### Player Data Management System

```csharp
using System.Collections.Generic;
using UnityEngine;
using SaveFramework.Runtime.Attributes;
using SaveFramework.Runtime.Core;

public class PlayerDataManager : MonoBehaviour
{
    [Header("Basic Attributes")]
    [Save("health")] public int Health = 100;
    [Save("mana")] public int Mana = 50;
    [Save("level")] public int Level = 1;
    [Save("experience")] public float Experience = 0f;

    [Header("Position Info")]
    [Save("position")] public Vector3 Position;
    [Save("rotation")] public Quaternion Rotation = Quaternion.identity;
    [Save("scene")] public string CurrentScene = "MainMenu";

    [Header("Equipment and Items")]
    [Save("inventory")] public List<int> Inventory = new List<int>();
    [Save("equipment")] public int[] Equipment = new int[6]; // Equipment slots
    [Save("currency")] public int Gold = 0;

    [Header("Game Settings")]
    [Save("difficulty")] public GameDifficulty Difficulty = GameDifficulty.Normal;
    [Save("master_volume")] public float MasterVolume = 1f;
    [Save("music_volume")] public float MusicVolume = 0.8f;
    [Save("sfx_volume")] public float SfxVolume = 1f;

    private void Start()
    {
        // Auto load on game start
        LoadGameData();
    }

    private void Update()
    {
        // Update position in real time
        Position = transform.position;
        Rotation = transform.rotation;
        
        // Hotkey save/load
        if (Input.GetKeyDown(KeyCode.F5))
            SaveGameData();
        
        if (Input.GetKeyDown(KeyCode.F9))
            LoadGameData();
    }

    public void SaveGameData(string slotName = "autosave")
    {
        try
        {
            SaveManager.Instance.Save(slotName);
            Debug.Log($"Game data saved to slot: {slotName}");
            
            // Show save success notification
            ShowSaveNotification("Save Successful!");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Save failed: {ex.Message}");
            ShowSaveNotification("Save Failed!");
        }
    }

    public void LoadGameData(string slotName = "autosave")
    {
        try
        {
            if (SaveManager.Instance.HasSave(slotName))
            {
                SaveManager.Instance.Load(slotName);
                
                // Apply loaded position
                transform.position = Position;
                transform.rotation = Rotation;
                
                Debug.Log($"Game data loaded from slot: {slotName}");
                ShowSaveNotification("Load Successful!");
            }
            else
            {
                Debug.LogWarning($"Save slot does not exist: {slotName}");
                ShowSaveNotification("Save does not exist!");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Load failed: {ex.Message}");
            ShowSaveNotification("Load Failed!");
        }
    }

    public void CreateNewGame()
    {
        // Reset all data to default values
        Health = 100;
        Mana = 50;
        Level = 1;
        Experience = 0f;
        Position = Vector3.zero;
        Rotation = Quaternion.identity;
        CurrentScene = "Tutorial";
        Inventory.Clear();
        Equipment = new int[6];
        Gold = 0;
        
        // Save new game state
        SaveGameData("newgame");
        
        Debug.Log("New game created");
    }

    private void ShowSaveNotification(string message)
    {
        // Implement UI notification logic
        Debug.Log($"[Notification] {message}");
    }
}

public enum GameDifficulty
{
    Easy,
    Normal,
    Hard,
    Nightmare
}
```

## Editor Tools

### Code Generation Tools

SaveFramework provides convenient editor tools:

#### 1. Generate Registration Code
**Menu Path**: Tools → Save Framework → Generate Registration

- Scans all fields using `[Save]` attribute in the project
- Generates optimized registration code to avoid runtime reflection
- Automatically creates `Assets/SaveFramework/Generated/SaveFramework.Generated.cs`

#### 2. Clear Generated Code
**Menu Path**: Tools → Save Framework → Clear Generated Registration

- Deletes auto-generated registration files
- Used for reset or cleanup project

#### 3. Show Generated File
**Menu Path**: Tools → Save Framework → Show Generated File

- Opens generated code file in file manager
- Convenient for viewing and debugging

#### 4. Pre-generate Converter Registry
**Menu Path**: Tools → Save Framework → Pre-generate Converter Registry

- Scans and registers all custom type converters
- Optimizes type conversion performance

## Performance Optimization Tips

### 1. Reduce Save Frequency
```csharp
public class OptimizedSaver : MonoBehaviour
{
    private float lastSaveTime;
    private const float SAVE_INTERVAL = 30f; // 30 second interval

    private void Update()
    {
        if (Time.time - lastSaveTime > SAVE_INTERVAL)
        {
            SaveManager.Instance.Save("autosave");
            lastSaveTime = Time.time;
        }
    }
}
```

### 2. Async Save
```csharp
using System.Collections;

public class AsyncSaver : MonoBehaviour
{
    public IEnumerator SaveAsync(string slotName)
    {
        yield return new WaitForEndOfFrame();
        
        try
        {
            SaveManager.Instance.Save(slotName);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Async save failed: {ex.Message}");
        }
    }

    public void InitiateSave(string slotName)
    {
        StartCoroutine(SaveAsync(slotName));
    }
}
```

## Troubleshooting

### Common Issues

#### 1. Data Not Saving
**Checklist:**
- [ ] Does GameObject have `SaveId` component?
- [ ] Are fields marked with `[Save]` attribute?
- [ ] Did you execute "Generate Registration"?
- [ ] Does save path have write permission?

**Solution:**
```csharp
// Check SaveId
var saveId = GetComponent<SaveId>();
if (saveId == null)
{
    saveId = gameObject.AddComponent<SaveId>();
}

// Verify save path
Debug.Log($"Save path: {Application.persistentDataPath}/Saves");
```

#### 2. Incorrect Data on Load
**Possible Causes:**
- Field type changed
- Save key name mismatch
- Save file corrupted

**Solution:**
```csharp
public void SafeLoad(string slotName)
{
    try
    {
        if (SaveManager.Instance.HasSave(slotName))
        {
            SaveManager.Instance.Load(slotName);
        }
        else
        {
            Debug.LogWarning($"Save {slotName} does not exist, using default values");
            UseDefaultValues();
        }
    }
    catch (System.Exception ex)
    {
        Debug.LogError($"Load failed, using default values: {ex.Message}");
        UseDefaultValues();
    }
}

private void UseDefaultValues()
{
    // Set safe default values
}
```

## API Reference

### SaveManager Class

#### Static Properties
- `Instance`: Get SaveManager singleton instance

#### Methods
- `Save(string slotName)`: Save all marked data to specified slot
- `Load(string slotName)`: Load data from specified slot
- `HasSave(string slotName)`: Check if specified save exists
- `DeleteSave(string slotName)`: Delete specified save
- `GetSaveSlots()`: Get all available save slots
- `SetBackend(ISaveBackend backend)`: Set custom save backend

### SaveAttribute Class

#### Constructors
- `SaveAttribute()`: Use field name as key
- `SaveAttribute(string key)`: Specify save key name
- `SaveAttribute(string key, params string[] aliases)`: Specify key name and aliases
- `SaveAttribute(Type converterType)`: Specify custom converter
- `SaveAttribute(string key, Type converterType, params string[] aliases)`: Full parameters

#### Properties
- `Key`: Save key name
- `Aliases`: Key name aliases array
- `ConverterType`: Custom converter type

### SaveId Class

#### Properties
- `Id`: Unique identifier string

#### Methods
- `GenerateNewId()`: Generate new unique identifier
- `SetCustomId(string customId)`: Set custom identifier
- `HasValidId()`: Check if has valid identifier
- `GetDisplayName()`: Get display name (for debugging)

## Best Practices

### 1. Naming Conventions
- Use meaningful save key names: `[Save("player_health")]` instead of `[Save("hp")]`
- Set aliases for important data to support data migration: `[Save("health", "hp", "life")]`
- Use descriptive custom IDs for SaveId: `"player_main"`, `"inventory_manager"`

### 2. Data Organization
```csharp
// Recommended: Organize by functional modules
public class PlayerCombatData : MonoBehaviour
{
    [Save] public int Health;
    [Save] public int Mana;
    [Save] public float AttackPower;
}

public class PlayerProgressData : MonoBehaviour
{
    [Save] public int Level;
    [Save] public float Experience;
    [Save] public int SkillPoints;
}
```

### 3. Error Handling
```csharp
public class RobustSaveManager : MonoBehaviour
{
    public bool TrySave(string slotName, out string errorMessage)
    {
        errorMessage = null;
        try
        {
            SaveManager.Instance.Save(slotName);
            return true;
        }
        catch (System.Exception ex)
        {
            errorMessage = ex.Message;
            return false;
        }
    }

    public bool TryLoad(string slotName, out string errorMessage)
    {
        errorMessage = null;
        try
        {
            SaveManager.Instance.Load(slotName);
            return true;
        }
        catch (System.Exception ex)
        {
            errorMessage = ex.Message;
            return false;
        }
    }
}
```

## License and Support

SaveFramework is an open source project. If you encounter issues or have suggestions for improvements, please contact us through:

- GitHub Issues: Submit bug reports and feature requests
- Project Documentation: View latest usage guides and API documentation

---

*This document was last updated: 2024*