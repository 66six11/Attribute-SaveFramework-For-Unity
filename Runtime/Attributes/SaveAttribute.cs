using System;

namespace SaveFramework
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class SaveAttribute : Attribute
    {
        public string Key { get; }
        public string[] Aliases { get; }

        public SaveAttribute()
        {
            Key = string.Empty;
            Aliases = Array.Empty<string>();
        }

        public SaveAttribute(string key, params string[] aliases)
        {
            Key = key ?? string.Empty;
            Aliases = aliases ?? Array.Empty<string>();
        }

        public string EffectiveKey(string fieldName)
        {
            return string.IsNullOrEmpty(Key) ? fieldName : Key;
        }
    }
}