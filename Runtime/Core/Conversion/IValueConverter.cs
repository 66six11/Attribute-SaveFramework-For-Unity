using System;

namespace SaveFramework.Runtime.Core.Conversion
{
    /// <summary>
    /// 值转换器的非通用接口，允许在注册表中按类型存储
    /// </summary>
    public interface IValueConverter
    {
        /// <summary>
        /// 此转换器处理的类型
        /// </summary>
        Type TargetType { get; }

        /// <summary>
        /// 将值转换为 JSON 可序列化格式
        /// </summary>
        object ToJsonValue(object value);

        /// <summary>
        ///将 JSON 值转换回目标类型
        /// </summary>
        object FromJsonValue(object jsonValue);
    }

    /// <summary>
    /// 强类型值转换器的通用接口
    /// </summary>
    public interface IValueConverter<T> : IValueConverter
    {
        /// <summary>
        /// 将强类型值转换为 JSON 可序列化格式
        /// </summary>
        object ToJsonValue(T value);

        /// <summary>
        /// 将 JSON 值转换回强类型目标
        /// </summary>
        T FromJsonValue(object jsonValue);
    }

    /// <summary>
    /// 用于实现值转换器的基类
    /// </summary>
    public abstract class ValueConverter<T> : IValueConverter<T>
    {
        public Type TargetType => typeof(T);

        public abstract object ToJsonValue(T value);
        public abstract T FromJsonValue(object jsonValue);

        // Non-generic interface implementations
        object IValueConverter.ToJsonValue(object value)
        {
            if (value is T typedValue)
                return ToJsonValue(typedValue);
            
            throw new ArgumentException($"Expected {typeof(T).Name}, got {value?.GetType().Name ?? "null"}");
        }

        object IValueConverter.FromJsonValue(object jsonValue)
        {
            return FromJsonValue(jsonValue);
        }
    }
}