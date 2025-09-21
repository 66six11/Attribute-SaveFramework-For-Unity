#if UNITY_EDITOR
using SaveFramework.Runtime.Attributes;
using SaveFramework.Runtime.Core.Conversion;
using UnityEditor;
using UnityEngine;

namespace SaveFramework.Editor
{
    [CustomPropertyDrawer(typeof(SaveAttribute))]
    public class SaveFieldDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var saveFieldAttr = attribute as SaveAttribute;
            
            // Check if the field type is supported
            var fieldType = fieldInfo.FieldType;
            bool isSupported = IsTypeSupported(fieldType);
            
            // Draw the property with warning if not supported
            if (!isSupported)
            {
                var originalColor = GUI.color;
                GUI.color = Color.red;
                
                EditorGUI.PropertyField(position, property, label);
                
                // Show warning message
                var warningRect = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight, position.width, EditorGUIUtility.singleLineHeight);
                EditorGUI.HelpBox(warningRect, $"类型 '{fieldType.Name}' SaveFramework 转换器不支持！, 请使用自定义转换器。", MessageType.Warning);
                
                GUI.color = originalColor;
            }
            else
            {
                EditorGUI.PropertyField(position, property, label);
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var fieldType = fieldInfo.FieldType;
            bool isSupported = IsTypeSupported(fieldType);
            
            // Add extra height for warning message if type is not supported
            return isSupported ? EditorGUIUtility.singleLineHeight : EditorGUIUtility.singleLineHeight * 2.5f;
        }

        private bool IsTypeSupported(System.Type type)
        {
            // Initialize converter registry if needed
            ConverterRegistry.Initialize();
            
            // Check primitive types
            if (type.IsPrimitive || type == typeof(string) || type.IsEnum)
                return true;
            
            // Check if there's a registered converter
            if (ConverterRegistry.HasConverter(type))
                return true;
            
            // Check arrays
            if (type.IsArray)
                return IsTypeSupported(type.GetElementType());
            
            // Check generic Lists
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(System.Collections.Generic.List<>))
                return IsTypeSupported(type.GetGenericArguments()[0]);
            
            return false;
        }
    }
}
#endif