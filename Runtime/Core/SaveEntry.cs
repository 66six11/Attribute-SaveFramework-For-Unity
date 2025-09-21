using System;
using System.Reflection;

namespace SaveFramework.Runtime.Core
{
    /// <summary>
    /// Represents a single saveable field entry
    /// </summary>
    public class SaveEntry
    {
        public string Key { get; }
        public string[] Aliases { get; }
        public Type ComponentType { get; }
        public string FieldName { get; }
        public Type FieldType { get; }
        public FieldInfo FieldInfo { get; }
        public Func<object, object> Getter { get; }
        public Action<object, object> Setter { get; }

        public SaveEntry(string key, string[] aliases, Type componentType, string fieldName, 
                        Type fieldType, FieldInfo fieldInfo, Func<object, object> getter = null, 
                        Action<object, object> setter = null)
        {
            Key = key;
            Aliases = aliases ?? Array.Empty<string>();
            ComponentType = componentType;
            FieldName = fieldName;
            FieldType = fieldType;
            FieldInfo = fieldInfo;
            Getter = getter;
            Setter = setter;
        }

        /// <summary>
        /// Get the value from the component instance
        /// </summary>
        public object GetValue(object component)
        {
            if (Getter != null)
                return Getter(component);
            
            return FieldInfo?.GetValue(component);
        }

        /// <summary>
        /// Set the value on the component instance
        /// </summary>
        public void SetValue(object component, object value)
        {
            if (Setter != null)
            {
                Setter(component, value);
                return;
            }

            FieldInfo?.SetValue(component, value);
        }

        /// <summary>
        /// Check if this entry matches the given key (including aliases)
        /// </summary>
        public bool MatchesKey(string searchKey)
        {
            if (Key == searchKey)
                return true;

            foreach (var alias in Aliases)
            {
                if (alias == searchKey)
                    return true;
            }

            return false;
        }
    }
}