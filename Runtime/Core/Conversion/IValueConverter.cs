using System;

namespace SaveFramework.Runtime.Core.Conversion
{
    /// <summary>
    /// Non-generic interface for value converters, allowing storage by Type in registry
    /// </summary>
    public interface IValueConverter
    {
        /// <summary>
        /// The type this converter handles
        /// </summary>
        Type TargetType { get; }

        /// <summary>
        /// Convert a value to JSON-serializable format
        /// </summary>
        object ToJsonValue(object value);

        /// <summary>
        /// Convert a JSON value back to the target type
        /// </summary>
        object FromJsonValue(object jsonValue);
    }

    /// <summary>
    /// Generic interface for strongly-typed value converters
    /// </summary>
    public interface IValueConverter<T> : IValueConverter
    {
        /// <summary>
        /// Convert a strongly-typed value to JSON-serializable format
        /// </summary>
        object ToJsonValue(T value);

        /// <summary>
        /// Convert a JSON value back to the strongly-typed target
        /// </summary>
        T FromJsonValue(object jsonValue);
    }

    /// <summary>
    /// Base class for implementing value converters
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