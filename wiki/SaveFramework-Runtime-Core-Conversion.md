# SaveFramework.Runtime.Core.Conversion

## AutoRegisterConverterAttribute

**Description:** 自动配准的标记转换器的属性

---

## ConverterRegistry

**Description:** Registry for value converters with automatic discovery and registration

### Methods and Properties

#### ClearConverters (Method)
清除所有已注册的转换器（用于测试）

#### GetConverter (Method)
获取指定类型的转换器

#### GetSupportedTypes (Method)
获取所有注册的转换器类型

#### HasConverter (Method)
检查是否存在指定类型的转换器

#### RegisterConverter (Method)
手动注册转换器

---

## IValueConverter

**Description:** 值转换器的非通用接口，允许在注册表中按类型存储

### Methods and Properties

#### FromJsonValue (Method)
将 JSON 值转换回目标类型

#### ToJsonValue (Method)
将值转换为 JSON 可序列化格式

#### TargetType (Property)
此转换器处理的类型

---

## IValueConverter

**Description:** 强类型值转换器的通用接口

### Methods and Properties

#### FromJsonValue (Method)
将 JSON 值转换回强类型目标

#### ToJsonValue (Method)
将强类型值转换为 JSON 可序列化格式

---

## NumericArrayReader

**Description:** 用于解析各种 JSON 和对象格式的数值数组的实用程序
处理 JArray、JObject{x，y，z，w}/{r，g，b，a}、托管数组、IEnumerable 和 Unity 类型

### Methods and Properties

#### ReadFloatArray (Method)
从各种输入格式解析浮点数组

#### ReadIntArray (Method)
解析来自各种输入格式的整数数组

---

## ValueConverter

**Description:** 用于实现值转换器的基类

---

*Last updated: 2025-09-22 08:39:21 UTC*
