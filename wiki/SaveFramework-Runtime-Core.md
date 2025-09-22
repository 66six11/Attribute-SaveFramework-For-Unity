# SaveFramework.Runtime.Core

[← Back to Home](Home.md)

## Table of Contents

- [Converters](#converters)
  - [FromJsonValue (Method)](#converters-fromjsonvalue-method)
  - [IsSupported (Method)](#converters-issupported-method)
  - [ToJsonValue (Method)](#converters-tojsonvalue-method)
- [FileBackend](#filebackend)
- [ISaveBackend](#isavebackend)
  - [DeleteSave (Method)](#isavebackend-deletesave-method)
  - [GetSaveSlots (Method)](#isavebackend-getsaveslots-method)
  - [HasSave (Method)](#isavebackend-hassave-method)
  - [Load (Method)](#isavebackend-load-method)
  - [Save (Method)](#isavebackend-save-method)
- [SaveData](#savedata)
  - [Clear (Method)](#savedata-clear-method)
  - [GetValue (Method)](#savedata-getvalue-method)
  - [HasKey (Method)](#savedata-haskey-method)
  - [RemoveKey (Method)](#savedata-removekey-method)
  - [SetValue (Method)](#savedata-setvalue-method)
- [SaveEntry](#saveentry)
  - [GetValue (Method)](#saveentry-getvalue-method)
  - [MatchesKey (Method)](#saveentry-matcheskey-method)
  - [SetValue (Method)](#saveentry-setvalue-method)
- [SaveManager](#savemanager)
  - [HasSave (Method)](#savemanager-hassave-method)
  - [Load (Method)](#savemanager-load-method)
  - [Load (Method)](#savemanager-load-method)
  - [Save (Method)](#savemanager-save-method)
  - [SetBackend (Method)](#savemanager-setbackend-method)
  - ... and 3 more

## Converters

**Description:** Facade for conversion system - routes to modular converter registry

### Methods and Properties

#### FromJsonValue (Method) {#converters-fromjsonvalue-method}
```csharp
object FromJsonValue(
    object jsonValue,
    Type targetType
)
```


**Description:** Convert a JSON value back to the target type

#### IsSupported (Method) {#converters-issupported-method}
```csharp
bool IsSupported(Type type)
```


**Description:** Check if a type is supported for serialization

#### ToJsonValue (Method) {#converters-tojsonvalue-method}
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

#### DeleteSave (Method) {#isavebackend-deletesave-method}
```csharp
void DeleteSave(string slotName)
```


**Description:** 删除保存槽

#### GetSaveSlots (Method) {#isavebackend-getsaveslots-method}
```csharp
string[] GetSaveSlots()
```


**Description:** 获取所有可用的保存槽名称

#### HasSave (Method) {#isavebackend-hassave-method}
```csharp
bool HasSave(string slotName)
```


**Description:** 检查是否存在保存槽

#### Load (Method) {#isavebackend-load-method}
```csharp
Dictionary<string, object> Load(string slotName)
```


**Description:** 从指定插槽加载数据

#### Save (Method) {#isavebackend-save-method}
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

#### Clear (Method) {#savedata-clear-method}
```csharp
void Clear()
```


**Description:** 清除所有数据

#### GetValue (Method) {#savedata-getvalue-method}
```csharp
object GetValue(
    string key,
    Type targetType,
    object defaultValue = null
)
```


**Description:** 获取给定键的值作为对象

#### HasKey (Method) {#savedata-haskey-method}
```csharp
bool HasKey(string key)
```


**Description:** 检查密钥是否存在

#### RemoveKey (Method) {#savedata-removekey-method}
```csharp
bool RemoveKey(string key)
```


**Description:** 从数据中删除key

#### SetValue (Method) {#savedata-setvalue-method}
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

#### GetValue (Method) {#saveentry-getvalue-method}
```csharp
object GetValue(object component)
```


**Description:** 从组件实例获取值

#### MatchesKey (Method) {#saveentry-matcheskey-method}
```csharp
bool MatchesKey(string searchKey)
```


**Description:** 检查此条目是否与给定的键（包括别名）匹配

#### SetValue (Method) {#saveentry-setvalue-method}
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

#### ClearCache (Method) {#savemanager-clearcache-method}
```csharp
void ClearCache()
```


**Description:** 清除内存中加载缓存的存档数据

#### DeleteSave (Method) {#savemanager-deletesave-method}
```csharp
void DeleteSave(string slotName)
```


**Description:** 删除保存槽

#### GetSaveSlots (Method) {#savemanager-getsaveslots-method}
```csharp
string[] GetSaveSlots()
```


**Description:** 获取所有可用的保存槽名称

#### HasSave (Method) {#savemanager-hassave-method}
```csharp
bool HasSave(string slotName)
```


**Description:** 检查是否存在保存槽

#### Load (Method) {#savemanager-load-method}
```csharp
void Load(string slotName)
```


**Description:** 从指定的插槽加载数据并应用于所有 SaveId 组件

#### Load (Method) {#savemanager-load-method}
```csharp
void Load(
    string slotName,
    SaveId saveId
)
```


**Description:** 加载指定 SaveId 组件的保存数据

#### Save (Method) {#savemanager-save-method}
```csharp
void Save(string slotName)
```


**Description:** 将所有 SaveId 组件保存到指定的插槽中

#### SetBackend (Method) {#savemanager-setbackend-method}
```csharp
void SetBackend(ISaveBackend customBackend)
```


**Description:** 设置自定义保存后端

---

*Last updated: 2025-09-22 09:17:25 UTC*
