using SaveFramework.Runtime.Core.Conversion;
using UnityEngine;

namespace SaveFramework.Runtime.Core.Conversion.BuiltIn
{
    [AutoRegisterConverter]
    public class Vector2Converter : ValueConverter<Vector2>
    {
        public override object ToJsonValue(Vector2 value)
        {
            return new float[] { value.x, value.y };
        }

        public override Vector2 FromJsonValue(object jsonValue)
        {
            var arr = NumericArrayReader.ReadFloatArray(jsonValue);
            return arr.Length >= 2 ? new Vector2(arr[0], arr[1]) : Vector2.zero;
        }
    }

    [AutoRegisterConverter]
    public class Vector3Converter : ValueConverter<Vector3>
    {
        public override object ToJsonValue(Vector3 value)
        {
            return new float[] { value.x, value.y, value.z };
        }

        public override Vector3 FromJsonValue(object jsonValue)
        {
            var arr = NumericArrayReader.ReadFloatArray(jsonValue);
            return arr.Length >= 3 ? new Vector3(arr[0], arr[1], arr[2]) : Vector3.zero;
        }
    }

    [AutoRegisterConverter]
    public class Vector4Converter : ValueConverter<Vector4>
    {
        public override object ToJsonValue(Vector4 value)
        {
            return new float[] { value.x, value.y, value.z, value.w };
        }

        public override Vector4 FromJsonValue(object jsonValue)
        {
            var arr = NumericArrayReader.ReadFloatArray(jsonValue);
            return arr.Length >= 4 ? new Vector4(arr[0], arr[1], arr[2], arr[3]) : Vector4.zero;
        }
    }

    [AutoRegisterConverter]
    public class QuaternionConverter : ValueConverter<Quaternion>
    {
        public override object ToJsonValue(Quaternion value)
        {
            return new float[] { value.x, value.y, value.z, value.w };
        }

        public override Quaternion FromJsonValue(object jsonValue)
        {
            var arr = NumericArrayReader.ReadFloatArray(jsonValue);
            return arr.Length >= 4 ? new Quaternion(arr[0], arr[1], arr[2], arr[3]) : Quaternion.identity;
        }
    }
}