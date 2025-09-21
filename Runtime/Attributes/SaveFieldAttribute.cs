using System;
using UnityEngine;

namespace SaveFramework.Runtime.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public class SaveFieldAttribute : Attribute
    {
        public string Key { get; }
        public string[] Aliases { get; }
        public Type ConverterType { get; }

        public SaveFieldAttribute()
        {
            Key = string.Empty;
            Aliases = new string[0];
            ConverterType = null;
        }

        public SaveFieldAttribute(string key, params string[] aliases)
        {
            Key = key ?? string.Empty;
            Aliases = aliases ?? new string[0];
            ConverterType = null;
        }

        public SaveFieldAttribute(Type converterType)
        {
            Key = string.Empty;
            Aliases = new string[0];
            ConverterType = converterType;
        }

        public SaveFieldAttribute(string key, Type converterType, params string[] aliases)
        {
            Key = key ?? string.Empty;
            Aliases = aliases ?? new string[0];
            ConverterType = converterType;
        }

        public string EffectiveKey(string fieldName)
        {
            return string.IsNullOrEmpty(Key) ? fieldName : Key;
        }
    }
}