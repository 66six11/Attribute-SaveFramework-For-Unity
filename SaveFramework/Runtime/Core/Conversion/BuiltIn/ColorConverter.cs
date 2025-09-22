using SaveFramework.Runtime.Core.Conversion;
using UnityEngine;

namespace SaveFramework.Runtime.Core.Conversion.BuiltIn
{
    [AutoRegisterConverter]
    public class ColorConverter : ValueConverter<Color>
    {
        public override object ToJsonValue(Color value)
        {
            return new float[] { value.r, value.g, value.b, value.a };
        }

        public override Color FromJsonValue(object jsonValue)
        {
            var arr = NumericArrayReader.ReadFloatArray(jsonValue);
            return arr.Length >= 4 ? new Color(arr[0], arr[1], arr[2], arr[3]) : 
                   arr.Length >= 3 ? new Color(arr[0], arr[1], arr[2], 1.0f) : Color.white;
        }
    }
}