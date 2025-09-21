# SaveFramework 使用说明

## 概述

SaveFramework 是一个专为 Unity 设计的轻量级、易用的存档系统框架。它通过简单的属性标记即可实现游戏数据的自动保存和加载，支持多种数据类型，并提供灵活的扩展机制。

## 主要特性

- ✅ **简单易用**: 只需添加 `[Save]` 属性即可标记需要保存的字段
- ✅ **自动序列化**: 自动处理 Unity 常用类型（Vector3、Quaternion、Color 等）
- ✅ **多存档槽**: 支持多个独立的存档槽位
- ✅ **类型安全**: 编译时检查，避免运行时错误
- ✅ **扩展性强**: 支持自定义类型转换器
- ✅ **性能优化**: 使用代码生成避免反射开销
- ✅ **跨平台**: 支持所有 Unity 目标平台

## 快速开始

### 1. 安装框架

将 SaveFramework 文件夹放入您的 Unity 项目的 Assets 目录中。

### 2. 基础设置

#### 添加 SaveId 组件
在需要保存数据的 GameObject 上添加 `SaveId` 组件：

```csharp
using UnityEngine;
using SaveFramework.Components;

public class Player : MonoBehaviour
{
    // SaveId 组件会自动添加唯一标识符
}
```

#### 标记需要保存的字段
使用 `[Save]` 属性标记需要保存的字段：

```csharp
using SaveFramework.Runtime.Attributes;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Save("health")] public int Health = 100;
    [Save("mana")] public int Mana = 50;
    [Save("level")] public int Level = 1;
    [Save] public Vector3 Position;  // 使用字段名作为键
}
```

#### 生成注册代码
在 Unity 编辑器中选择：**Tools → Save Framework → 生成注册**

这将扫描所有带有 `[Save]` 属性的字段并生成必要的注册代码。

### 3. 保存和加载数据

```csharp
using SaveFramework.Runtime.Core;

// 保存数据到指定槽位
SaveManager.Instance.Save("player_save");

// 从指定槽位加载数据
SaveManager.Instance.Load("player_save");

// 检查是否存在存档
bool hasSave = SaveManager.Instance.HasSave("player_save");

// 删除存档
SaveManager.Instance.DeleteSave("player_save");
```

## 详细使用指南

### Save 属性详解

#### 基本用法

```csharp
public class GameData : MonoBehaviour
{
    // 使用自定义键名
    [Save("player_hp")] public int Health = 100;
    
    // 使用字段名作为键（推荐用于简单情况）
    [Save] public int Mana = 50;
    
    // 支持别名（用于数据迁移）
    [Save("exp", "experience", "xp")] public float Experience = 0f;
}
```

#### 支持的数据类型

**基础类型：**
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

**Unity 类型：**
```csharp
[Save] public Vector2 Position2D;
[Save] public Vector3 Position3D;
[Save] public Vector4 Position4D;
[Save] public Quaternion Rotation;
[Save] public Color PlayerColor;
```

**数组和集合：**
```csharp
[Save] public int[] IntArray;
[Save] public Vector3[] Waypoints;
[Save] public List<string> ItemNames;
[Save] public Color[] FavoriteColors;
```

**枚举类型：**
```csharp
public enum Difficulty { Easy, Normal, Hard }

[Save] public Difficulty GameDifficulty;
```

### SaveId 组件详解

`SaveId` 组件为每个 GameObject 提供唯一标识符，是存档系统的核心组件。

#### 自动生成 ID
```csharp
// SaveId 会在组件添加时自动生成唯一 ID
var saveId = gameObject.AddComponent<SaveId>();
Debug.Log(saveId.Id); // 输出类似: "a1b2c3d4-e5f6-7890-abcd-ef1234567890"
```

#### 自定义 ID
```csharp
var saveId = GetComponent<SaveId>();
saveId.SetCustomId("player_main"); // 设置可预测的 ID
```

#### 实用方法
```csharp
var saveId = GetComponent<SaveId>();

// 检查是否有有效 ID
if (saveId.HasValidId())
{
    Debug.Log("SaveId 已设置");
}

// 获取显示名称（用于调试）
Debug.Log(saveId.GetDisplayName()); // 输出: "Player (a1b2c3d4...)"

// 重新生成 ID
saveId.GenerateNewId();
```

### 存档后端配置

#### 默认文件后端
框架默认使用文件系统存储存档，位置为 `Application.persistentDataPath/Saves/`。

#### 自定义存档目录
```csharp
using SaveFramework.Runtime.Core;

public class CustomSaveSetup : MonoBehaviour
{
    void Awake()
    {
        // 设置自定义存档目录
        var customBackend = new FileBackend("/custom/save/path");
        SaveManager.Instance.SetBackend(customBackend);
    }
}
```

#### 实现自定义后端
```csharp
using SaveFramework.Runtime.Core;
using System.Collections.Generic;

public class CloudSaveBackend : ISaveBackend
{
    public void Save(string slotName, Dictionary<string, object> data)
    {
        // 实现云存档保存逻辑
    }
    
    public Dictionary<string, object> Load(string slotName)
    {
        // 实现云存档加载逻辑
        return new Dictionary<string, object>();
    }
    
    public bool HasSave(string slotName)
    {
        // 检查云端是否存在存档
        return false;
    }
    
    public void DeleteSave(string slotName)
    {
        // 删除云端存档
    }
    
    public string[] GetSaveSlots()
    {
        // 获取所有云端存档槽位
        return new string[0];
    }
}
```

### 高级功能

#### 自定义类型转换器

对于框架不原生支持的类型，可以实现自定义转换器：

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

#### 带转换器的字段标记
```csharp
public class GameSession : MonoBehaviour
{
    [Save("start_time", typeof(DateTimeConverter))] 
    public DateTime StartTime = DateTime.Now;
}
```

#### 条件保存

通过继承和重写实现条件保存：

```csharp
public class ConditionalSave : MonoBehaviour
{
    [Save] public int Score = 0;
    [Save] public bool IsHighScore = false;
    
    public void UpdateScore(int newScore)
    {
        Score = newScore;
        IsHighScore = Score > PlayerPrefs.GetInt("HighScore", 0);
        
        // 只在高分时保存
        if (IsHighScore)
        {
            SaveManager.Instance.Save("high_score_slot");
        }
    }
}
```

## 完整示例

### 玩家数据管理系统

```csharp
using System.Collections.Generic;
using UnityEngine;
using SaveFramework.Runtime.Attributes;
using SaveFramework.Runtime.Core;

public class PlayerDataManager : MonoBehaviour
{
    [Header("基础属性")]
    [Save("health")] public int Health = 100;
    [Save("mana")] public int Mana = 50;
    [Save("level")] public int Level = 1;
    [Save("experience")] public float Experience = 0f;

    [Header("位置信息")]
    [Save("position")] public Vector3 Position;
    [Save("rotation")] public Quaternion Rotation = Quaternion.identity;
    [Save("scene")] public string CurrentScene = "MainMenu";

    [Header("装备和物品")]
    [Save("inventory")] public List<int> Inventory = new List<int>();
    [Save("equipment")] public int[] Equipment = new int[6]; // 装备槽位
    [Save("currency")] public int Gold = 0;

    [Header("游戏设置")]
    [Save("difficulty")] public GameDifficulty Difficulty = GameDifficulty.Normal;
    [Save("master_volume")] public float MasterVolume = 1f;
    [Save("music_volume")] public float MusicVolume = 0.8f;
    [Save("sfx_volume")] public float SfxVolume = 1f;

    private void Start()
    {
        // 游戏启动时自动加载
        LoadGameData();
    }

    private void Update()
    {
        // 实时更新位置
        Position = transform.position;
        Rotation = transform.rotation;
        
        // 快捷键保存/加载
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
            Debug.Log($"游戏数据已保存到槽位: {slotName}");
            
            // 显示保存成功提示
            ShowSaveNotification("保存成功!");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"保存失败: {ex.Message}");
            ShowSaveNotification("保存失败!");
        }
    }

    public void LoadGameData(string slotName = "autosave")
    {
        try
        {
            if (SaveManager.Instance.HasSave(slotName))
            {
                SaveManager.Instance.Load(slotName);
                
                // 应用加载的位置
                transform.position = Position;
                transform.rotation = Rotation;
                
                Debug.Log($"游戏数据已从槽位加载: {slotName}");
                ShowSaveNotification("加载成功!");
            }
            else
            {
                Debug.LogWarning($"存档槽位不存在: {slotName}");
                ShowSaveNotification("存档不存在!");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"加载失败: {ex.Message}");
            ShowSaveNotification("加载失败!");
        }
    }

    public void CreateNewGame()
    {
        // 重置所有数据到默认值
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
        
        // 保存新游戏状态
        SaveGameData("newgame");
        
        Debug.Log("新游戏已创建");
    }

    public bool HasAnySaveData()
    {
        var slots = SaveManager.Instance.GetSaveSlots();
        return slots.Length > 0;
    }

    public string[] GetAvailableSaveSlots()
    {
        return SaveManager.Instance.GetSaveSlots();
    }

    private void ShowSaveNotification(string message)
    {
        // 实现 UI 通知逻辑
        Debug.Log($"[通知] {message}");
    }

    // 自动保存功能
    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            SaveGameData("autosave_pause");
        }
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus)
        {
            SaveGameData("autosave_focus");
        }
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

### 场景切换时的数据持久化

```csharp
using UnityEngine;
using UnityEngine.SceneManagement;
using SaveFramework.Runtime.Core;

public class SceneTransitionManager : MonoBehaviour
{
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    private void OnSceneUnloaded(Scene scene)
    {
        // 场景卸载时自动保存
        SaveManager.Instance.Save("scene_transition");
        Debug.Log($"场景 {scene.name} 卸载时已保存数据");
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 新场景加载后自动恢复数据
        if (SaveManager.Instance.HasSave("scene_transition"))
        {
            SaveManager.Instance.Load("scene_transition");
            Debug.Log($"场景 {scene.name} 加载时已恢复数据");
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
    }
}
```

## 编辑器工具

### 代码生成工具

SaveFramework 提供了便捷的编辑器工具：

#### 1. 生成注册代码
**菜单路径**: Tools → Save Framework → 生成注册

- 扫描项目中所有使用 `[Save]` 属性的字段
- 生成优化的注册代码，避免运行时反射
- 自动创建 `Assets/SaveFramework/Generated/SaveFramework.Generated.cs`

#### 2. 清除生成的代码
**菜单路径**: Tools → Save Framework → 清除生成的注册

- 删除自动生成的注册文件
- 用于重置或清理项目

#### 3. 显示生成的文件
**菜单路径**: Tools → Save Framework → 显示生成的文件

- 在文件管理器中打开生成的代码文件
- 便于查看和调试

#### 4. 预生成转换器注册表
**菜单路径**: Tools → Save Framework → 预生成转换器注册表

- 扫描并注册所有自定义类型转换器
- 优化类型转换性能

### Inspector 扩展

框架为 `[Save]` 属性提供了特殊的 Inspector 显示：

- 显示保存键名
- 高亮显示可保存字段
- 类型兼容性检查

## 性能优化建议

### 1. 减少保存频率
```csharp
public class OptimizedSaver : MonoBehaviour
{
    private float lastSaveTime;
    private const float SAVE_INTERVAL = 30f; // 30秒间隔

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

### 2. 异步保存
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
            Debug.LogError($"异步保存失败: {ex.Message}");
        }
    }

    public void InitiateSave(string slotName)
    {
        StartCoroutine(SaveAsync(slotName));
    }
}
```

### 3. 选择性保存
```csharp
public class SelectiveSaver : MonoBehaviour
{
    [Save] public int ImportantData;
    public int TemporaryData; // 不保存临时数据

    public void SaveOnlyImportantData()
    {
        // 只有标记了 [Save] 的字段会被保存
        SaveManager.Instance.Save("important_only");
    }
}
```

## 故障排除

### 常见问题

#### 1. 数据没有保存
**检查清单：**
- [ ] GameObject 是否有 `SaveId` 组件？
- [ ] 字段是否标记了 `[Save]` 属性？
- [ ] 是否执行了"生成注册"？
- [ ] 保存路径是否有写入权限？

**解决方案：**
```csharp
// 检查 SaveId
var saveId = GetComponent<SaveId>();
if (saveId == null)
{
    saveId = gameObject.AddComponent<SaveId>();
}

// 验证保存路径
Debug.Log($"保存路径: {Application.persistentDataPath}/Saves");
```

#### 2. 加载时数据不正确
**可能原因：**
- 字段类型改变
- 保存键名不匹配
- 存档文件损坏

**解决方案：**
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
            Debug.LogWarning($"存档 {slotName} 不存在，使用默认值");
            UseDefaultValues();
        }
    }
    catch (System.Exception ex)
    {
        Debug.LogError($"加载失败，使用默认值: {ex.Message}");
        UseDefaultValues();
    }
}

private void UseDefaultValues()
{
    // 设置安全的默认值
}
```

#### 3. 类型不支持错误
**解决方案：**
创建自定义转换器：
```csharp
[AutoRegisterConverter]
public class CustomTypeConverter : IValueConverter
{
    public bool CanConvert(Type type)
    {
        return type == typeof(YourCustomType);
    }

    public object ToJsonValue(object value)
    {
        // 将自定义类型转换为可序列化格式
        var custom = (YourCustomType)value;
        return new { custom.Property1, custom.Property2 };
    }

    public object FromJsonValue(object jsonValue, Type targetType)
    {
        // 从序列化格式还原自定义类型
        dynamic data = jsonValue;
        return new YourCustomType
        {
            Property1 = data.Property1,
            Property2 = data.Property2
        };
    }
}
```

### 调试技巧

#### 1. 启用详细日志
```csharp
public class DebugSaver : MonoBehaviour
{
    [SerializeField] private bool enableDebugLogs = true;

    public void DebugSave(string slotName)
    {
        if (enableDebugLogs)
        {
            Debug.Log($"开始保存到槽位: {slotName}");
        }

        try
        {
            SaveManager.Instance.Save(slotName);
            
            if (enableDebugLogs)
            {
                Debug.Log($"保存成功: {slotName}");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"保存失败: {ex.Message}\n{ex.StackTrace}");
        }
    }
}
```

#### 2. 验证存档内容
```csharp
public void ValidateSaveData(string slotName)
{
    if (SaveManager.Instance.HasSave(slotName))
    {
        var filePath = Path.Combine(Application.persistentDataPath, "Saves", $"{slotName}.json");
        if (File.Exists(filePath))
        {
            var content = File.ReadAllText(filePath);
            Debug.Log($"存档内容:\n{content}");
        }
    }
}
```

## API 参考

### SaveManager 类

#### 静态属性
- `Instance`: 获取 SaveManager 单例实例

#### 方法
- `Save(string slotName)`: 保存所有标记数据到指定槽位
- `Load(string slotName)`: 从指定槽位加载数据
- `HasSave(string slotName)`: 检查是否存在指定存档
- `DeleteSave(string slotName)`: 删除指定存档
- `GetSaveSlots()`: 获取所有可用存档槽位
- `SetBackend(ISaveBackend backend)`: 设置自定义存档后端

### SaveAttribute 类

#### 构造函数
- `SaveAttribute()`: 使用字段名作为键
- `SaveAttribute(string key)`: 指定保存键名
- `SaveAttribute(string key, params string[] aliases)`: 指定键名和别名
- `SaveAttribute(Type converterType)`: 指定自定义转换器
- `SaveAttribute(string key, Type converterType, params string[] aliases)`: 完整参数

#### 属性
- `Key`: 保存键名
- `Aliases`: 键名别名数组
- `ConverterType`: 自定义转换器类型

### SaveId 类

#### 属性
- `Id`: 唯一标识符字符串

#### 方法
- `GenerateNewId()`: 生成新的唯一标识符
- `SetCustomId(string customId)`: 设置自定义标识符
- `HasValidId()`: 检查是否有有效标识符
- `GetDisplayName()`: 获取显示名称（用于调试）

### ISaveBackend 接口

#### 方法
- `Save(string slotName, Dictionary<string, object> data)`: 保存数据
- `Load(string slotName)`: 加载数据
- `HasSave(string slotName)`: 检查存档是否存在
- `DeleteSave(string slotName)`: 删除存档
- `GetSaveSlots()`: 获取所有存档槽位

## 最佳实践

### 1. 命名约定
- 使用有意义的保存键名：`[Save("player_health")]` 而不是 `[Save("hp")]`
- 为重要数据设置别名以支持数据迁移：`[Save("health", "hp", "life")]`
- SaveId 使用描述性的自定义 ID：`"player_main"`, `"inventory_manager"`

### 2. 数据组织
```csharp
// 推荐：按功能模块组织
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

### 3. 错误处理
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

### 4. 版本控制
```csharp
public class VersionedSaveData : MonoBehaviour
{
    [Save("save_version")] public int SaveVersion = 1;
    [Save] public int PlayerData;

    private void Start()
    {
        // 检查存档版本兼容性
        if (SaveVersion < 1)
        {
            MigrateFromOldVersion();
        }
    }

    private void MigrateFromOldVersion()
    {
        // 实现数据迁移逻辑
        Debug.Log("正在迁移旧版本存档数据...");
        SaveVersion = 1;
    }
}
```

## 许可证和支持

SaveFramework 是一个开源项目。如果您在使用过程中遇到问题或有改进建议，请通过以下方式联系：

- GitHub Issues: 提交 bug 报告和功能请求
- 项目文档: 查看最新的使用指南和 API 文档

## 更新日志

### 版本历史
- **v1.0.0**: 初始版本发布
  - 基础保存/加载功能
  - Unity 常用类型支持
  - 文件系统后端
  - 编辑器代码生成工具

---

*本文档最后更新：2024年*