using Newtonsoft.Json.Linq;
using UnityEngine;

namespace SaveFramework.Runtime.Core.Conversion.BuiltIn
{
    /// <summary>
    /// Bounds 转换器
    /// 序列化为 float[6]：{ cx, cy, cz, sx, sy, sz }
    /// 反序列化时支持：
    /// - float 数组（长度 >= 6）
    /// - JObject { center: [x,y,z]|{x,y,z}, size: [x,y,z]|{x,y,z} }
    /// - JObject { min: [x,y,z]|{x,y,z}, max: [x,y,z]|{x,y,z} }
    /// </summary>
    [AutoRegisterConverter]
    public class BoundsConverter : ValueConverter<Bounds>
    {
        public override object ToJsonValue(Bounds value)
        {
            var c = value.center;
            var s = value.size;
            return new float[] { c.x, c.y, c.z, s.x, s.y, s.z };
        }

        public override Bounds FromJsonValue(object jsonValue)
        {
            // 先处理对象形式（兼容 {center:{}, size:{}} 或 {min:{}, max:{}}）
            if (jsonValue is JObject jobj)
            {
                // 1) center + size
                if (jobj.TryGetValue("center", System.StringComparison.OrdinalIgnoreCase, out var cTok) &&
                    jobj.TryGetValue("size", System.StringComparison.OrdinalIgnoreCase, out var sTok))
                {
                    var cArr = NumericArrayReader.ReadFloatArray(cTok.ToObject<object>());
                    var sArr = NumericArrayReader.ReadFloatArray(sTok.ToObject<object>());

                    if (cArr.Length >= 3 && sArr.Length >= 3)
                    {
                        var center = new Vector3(cArr[0], cArr[1], cArr[2]);
                        var size   = new Vector3(sArr[0], sArr[1], sArr[2]);
                        return new Bounds(center, size);
                    }

                    return default; // 解析失败则返回默认
                }

                // 2) min + max
                if (jobj.TryGetValue("min", System.StringComparison.OrdinalIgnoreCase, out var minTok) &&
                    jobj.TryGetValue("max", System.StringComparison.OrdinalIgnoreCase, out var maxTok))
                {
                    var minArr = NumericArrayReader.ReadFloatArray(minTok.ToObject<object>());
                    var maxArr = NumericArrayReader.ReadFloatArray(maxTok.ToObject<object>());

                    if (minArr.Length >= 3 && maxArr.Length >= 3)
                    {
                        var min = new Vector3(minArr[0], minArr[1], minArr[2]);
                        var max = new Vector3(maxArr[0], maxArr[1], maxArr[2]);

                        var center = (min + max) * 0.5f;
                        var size   = new Vector3(Mathf.Abs(max.x - min.x),
                                                 Mathf.Abs(max.y - min.y),
                                                 Mathf.Abs(max.z - min.z));
                        return new Bounds(center, size);
                    }

                    return default;
                }
            }

            // 通用数组/枚举形式：期望至少 6 个数字
            var arr = NumericArrayReader.ReadFloatArray(jsonValue);
            if (arr.Length >= 6)
            {
                var center = new Vector3(arr[0], arr[1], arr[2]);
                var size   = new Vector3(arr[3], arr[4], arr[5]);
                return new Bounds(center, size);
            }

            // 不足则返回默认
            return default;
        }
    }
}