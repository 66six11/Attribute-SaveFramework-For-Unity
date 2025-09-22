# SaveFramework.Runtime.Core

## Converters

**Description:** Facade for conversion system - routes to modular converter registry

### Methods and Properties

#### FromJsonValue (Method)
Convert a JSON value back to the target type

#### IsSupported (Method)
Check if a type is supported for serialization

#### ToJsonValue (Method)
Convert a value to a JSON-serializable format

---

## FileBackend

**Description:** 使用 JSON 格式的基于文件的保存后端

---

## ISaveBackend

**Description:** 保存数据存储后端的接口

### Methods and Properties

#### DeleteSave (Method)
删除保存槽

#### GetSaveSlots (Method)
获取所有可用的保存槽名称

#### HasSave (Method)
检查是否存在保存槽

#### Load (Method)
从指定插槽加载数据

#### Save (Method)
将数据保存到指定插槽

**Parameters:**
- `slotName`: 保存槽名称
- `data`: 要保存的数据
- `reWrite`: 完全覆盖或增量保存

---

## SaveData

**Description:** 用于保存数据的容器，具有类型安全访问

### Methods and Properties

#### Clear (Method)
清除所有数据

#### GetValue (Method)
获取给定键的值作为对象

#### HasKey (Method)
检查密钥是否存在

#### RemoveKey (Method)
从数据中删除key

#### SetValue (Method)
Set a value for the given key

---

## SaveEntry

**Description:** Represents a single saveable field entry

### Methods and Properties

#### GetValue (Method)
从组件实例获取值

#### MatchesKey (Method)
检查此条目是否与给定的键（包括别名）匹配

#### SetValue (Method)
在组件实例上设置值

---

## SaveManager

**Description:** 保存框架的主管理器

### Methods and Properties

#### ClearCache (Method)
清除内存中加载缓存的存档数据

#### DeleteSave (Method)
删除保存槽

#### GetSaveSlots (Method)
获取所有可用的保存槽名称

#### HasSave (Method)
检查是否存在保存槽

#### Load (Method)
从指定的插槽加载数据并应用于所有 SaveId 组件

#### Load (Method)
加载指定 SaveId 组件的保存数据

#### Save (Method)
将所有 SaveId 组件保存到指定的插槽中

#### SetBackend (Method)
设置自定义保存后端

---

*Last updated: 2025-09-22 08:30:47 UTC*
