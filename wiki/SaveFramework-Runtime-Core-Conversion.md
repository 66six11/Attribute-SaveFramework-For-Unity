# SaveFramework.Runtime.Core.Conversion

[← Back to Home](Home.md)

## Table of Contents

- [AutoRegisterConverterAttribute](#autoregisterconverterattribute)
- [ConverterRegistry](#converterregistry)
  - [ClearConverters (Method)](#converterregistry-clearconverters-method)
  - [GetConverter (Method)](#converterregistry-getconverter-method)
  - [GetSupportedTypes (Method)](#converterregistry-getsupportedtypes-method)
  - [HasConverter (Method)](#converterregistry-hasconverter-method)
  - [RegisterConverter (Method)](#converterregistry-registerconverter-method)
- [IValueConverter](#ivalueconverter)
  - [FromJsonValue (Method)](#ivalueconverter-fromjsonvalue-method)
  - [TargetType (Property)](#ivalueconverter-targettype-property)
  - [ToJsonValue (Method)](#ivalueconverter-tojsonvalue-method)
- [IValueConverter](#ivalueconverter)
  - [FromJsonValue (Method)](#ivalueconverter-fromjsonvalue-method)
  - [ToJsonValue (Method)](#ivalueconverter-tojsonvalue-method)
- [NumericArrayReader](#numericarrayreader)
  - [ReadFloatArray (Method)](#numericarrayreader-readfloatarray-method)
  - [ReadIntArray (Method)](#numericarrayreader-readintarray-method)
- [ValueConverter](#valueconverter)

## AutoRegisterConverterAttribute

**Description:** 自动配准的标记转换器的属性

---

## ConverterRegistry

**Description:** Registry for value converters with automatic discovery and registration

### Methods and Properties

#### ClearConverters (Method) {#converterregistry-clearconverters-method}
```csharp
void ClearConverters()
```


**Description:** 清除所有已注册的转换器（用于测试）

#### GetConverter (Method) {#converterregistry-getconverter-method}
```csharp
IValueConverter GetConverter(Type type)
```


**Description:** 获取指定类型的转换器

#### GetSupportedTypes (Method) {#converterregistry-getsupportedtypes-method}
```csharp
Type[] GetSupportedTypes()
```


**Description:** 获取所有注册的转换器类型

#### HasConverter (Method) {#converterregistry-hasconverter-method}
```csharp
bool HasConverter(Type type)
```


**Description:** 检查是否存在指定类型的转换器

#### RegisterConverter (Method) {#converterregistry-registerconverter-method}
```csharp
void RegisterConverter(IValueConverter converter)
```


**Description:** 手动注册转换器

---

## IValueConverter

**Description:** 值转换器的非通用接口，允许在注册表中按类型存储

### Methods and Properties

#### FromJsonValue (Method) {#ivalueconverter-fromjsonvalue-method}
```csharp
object FromJsonValue(object jsonValue)
```


**Description:** 将 JSON 值转换回目标类型

#### ToJsonValue (Method) {#ivalueconverter-tojsonvalue-method}
```csharp
object ToJsonValue(object value)
```


**Description:** 将值转换为 JSON 可序列化格式

#### TargetType (Property) {#ivalueconverter-targettype-property}
```csharp
Type TargetType { get; set; }
```

**Description:** 此转换器处理的类型

---

## IValueConverter

**Description:** 强类型值转换器的通用接口

### Methods and Properties

#### FromJsonValue (Method) {#ivalueconverter-fromjsonvalue-method}
```csharp
T FromJsonValue(object jsonValue)
```


**Description:** 将 JSON 值转换回强类型目标

#### ToJsonValue (Method) {#ivalueconverter-tojsonvalue-method}
```csharp
object ToJsonValue(T value)
```


**Description:** 将强类型值转换为 JSON 可序列化格式

---

## NumericArrayReader

**Description:** 用于解析各种 JSON 和对象格式的数值数组的实用程序
处理 JArray、JObject{x，y，z，w}/{r，g，b，a}、托管数组、IEnumerable 和 Unity 类型

### Methods and Properties

#### ReadFloatArray (Method) {#numericarrayreader-readfloatarray-method}
```csharp
float[] ReadFloatArray(object obj)
```


**Description:** 从各种输入格式解析浮点数组

#### ReadIntArray (Method) {#numericarrayreader-readintarray-method}
```csharp
int[] ReadIntArray(object obj)
```


**Description:** 解析来自各种输入格式的整数数组

---

## ValueConverter

**Description:** 用于实现值转换器的基类

---

*Last updated: 2025-09-22 09:28:00 UTC*
