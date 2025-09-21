using System;
using UnityEngine;

namespace SaveFramework.Runtime.Attributes
{
    // 合并：一个标记同时服务运行时与编辑器（Inspector Drawer）
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class SaveAttribute : PropertyAttribute
    {
        public string Key { get; }
        public string[] Aliases { get; }
        // 可选：若需为某个字段指定专用转换器（实现 IValueConverter）
        public Type ConverterType { get; }

        public SaveAttribute()
        {
            Key = string.Empty;
            Aliases = Array.Empty<string>();
            ConverterType = null;
        }

        public SaveAttribute(string key, params string[] aliases)
        {
            Key = key ?? string.Empty;
            Aliases = aliases ?? Array.Empty<string>();
            ConverterType = null;
        }

        public SaveAttribute(Type converterType)
        {
            Key = string.Empty;
            Aliases = Array.Empty<string>();
            ConverterType = converterType;
        }

        public SaveAttribute(string key, Type converterType, params string[] aliases)
        {
            Key = key ?? string.Empty;
            Aliases = aliases ?? Array.Empty<string>();
            ConverterType = converterType;
        }

        public string EffectiveKey(string fieldName)
        {
            return string.IsNullOrEmpty(Key) ? fieldName : Key;
        }
    }
}