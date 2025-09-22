# SaveFramework.Runtime.Core.Conversion.BuiltIn

## BoundsConverter

**Description:** Bounds 转换器
序列化为 float[6]：{ cx, cy, cz, sx, sy, sz }
反序列化时支持：
- float 数组（长度 >= 6）
- JObject { center: [x,y,z]|{x,y,z}, size: [x,y,z]|{x,y,z} }
- JObject { min: [x,y,z]|{x,y,z}, max: [x,y,z]|{x,y,z} }

---

*Last updated: 2025-09-22 08:30:47 UTC*
