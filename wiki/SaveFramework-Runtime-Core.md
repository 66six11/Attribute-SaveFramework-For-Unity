# SaveFramework.Runtime.Core

[← Back to Home](Home.md)

## Converters

**Description:** Facade for conversion system - routes to modular converter registry

### Methods and Properties

#### FromJsonValue (Method)
```csharp
object FromJsonValue(
    object jsonValue,
    Type targetType
)
```


**Description:** Convert a JSON value back to the target type

#### IsSupported (Method)
```csharp
bool IsSupported(Type type)
```


**Description:** Check if a type is supported for serialization

#### ToJsonValue (Method)
```csharp
object ToJsonValue(
    object value,
    Type type
)
```


**Description:** Convert a value to a JSON-serializable format

---

## FileBackend

**Description:** 使用 JSON 格式的基于文件的保存后端

---

## ISaveBackend

**Description:** 保存数据存储后端的接口

### Methods and Properties

#### DeleteSave (Method)
```csharp
void DeleteSave(string slotName)
```


**Description:** 删除保存槽

#### GetSaveSlots (Method)
```csharp
string[] GetSaveSlots()
```


**Description:** 获取所有可用的保存槽名称

#### HasSave (Method)
```csharp
bool HasSave(string slotName)
```


**Description:** 检查是否存在保存槽

#### Load (Method)
```csharp
Dictionary<string, object> Load(string slotName)
```


**Description:** 从指定插槽加载数据

#### Save (Method)
```csharp
void Save(
    string slotName,
    Dictionary<string, object> data,
    bool reWrite = true
)
```


**Description:** 将数据保存到指定插槽

**Parameters:**
- `slotName` (`string`): 保存槽名称
- `data` (`Dictionary<string, object>`): 要保存的数据
- `reWrite`: 完全覆盖或增量保存

---

## SaveData

**Description:** 用于保存数据的容器，具有类型安全访问

### Methods and Properties

#### Clear (Method)
```csharp
void Clear()
```


**Description:** 清除所有数据

#### GetValue (Method)
```csharp
object GetValue(
    string key,
    Type targetType,
    object defaultValue = null
)
```


**Description:** 获取给定键的值作为对象

#### HasKey (Method)
```csharp
bool HasKey(string key)
```


**Description:** 检查密钥是否存在

#### RemoveKey (Method)
```csharp
bool RemoveKey(string key)
```


**Description:** 从数据中删除key

#### SetValue (Method)
```csharp
void SetValue(
    string key,
    object value
)
```


**Description:** Set a value for the given key

---

## SaveEntry

**Description:** Represents a single saveable field entry

### Methods and Properties

#### GetValue (Method)
```csharp
object GetValue(object component)
```


**Description:** 从组件实例获取值

#### MatchesKey (Method)
```csharp
bool MatchesKey(string searchKey)
```


**Description:** 检查此条目是否与给定的键（包括别名）匹配

#### SetValue (Method)
```csharp
void SetValue(
    object component,
    object value
)
```


**Description:** 在组件实例上设置值

---

## SaveManager

**Description:** 保存框架的主管理器

### Methods and Properties

#### ClearCache (Method)
```csharp
void ClearCache()
```


**Description:** 清除内存中加载缓存的存档数据

#### DeleteSave (Method)
```csharp
void DeleteSave(string slotName)
```


**Description:** 删除保存槽

#### GetSaveSlots (Method)
```csharp
string[] GetSaveSlots()
```


**Description:** 获取所有可用的保存槽名称

#### HasSave (Method)
```csharp
bool HasSave(string slotName)
```


**Description:** 检查是否存在保存槽

#### Load (Method)
```csharp
void Load(string slotName)
```


**Description:** 从指定的插槽加载数据并应用于所有 SaveId 组件

#### Load (Method)
```csharp
void Load(
    string slotName,
    SaveId saveId
)
```


**Description:** 加载指定 SaveId 组件的保存数据

#### Save (Method)
```csharp
void Save(string slotName)
```


**Description:** 将所有 SaveId 组件保存到指定的插槽中

#### SetBackend (Method)
```csharp
void SetBackend(ISaveBackend customBackend)
```


**Description:** 设置自定义保存后端

---

*Last updated: 2025-09-22 09:48:13 UTC*
