#if UNITY_EDITOR
using SaveFramework.Runtime.Attributes;
using SaveFramework.Runtime.Core.Conversion;
using SaveFramework.Runtime.Core.Conversion.Generated;
using UnityEditor;
using UnityEngine;

namespace SaveFramework.Editor
{
    [CustomPropertyDrawer(typeof(SaveAttribute))]
    public class SaveFieldDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            using (new EditorGUI.PropertyScope(position, label, property))
            {
                var fieldType = fieldInfo.FieldType;
                bool isSupported = IsTypeSupported(fieldType);

                // 实际的属性高度（支持多行、数组、展开）
                float propHeight = EditorGUI.GetPropertyHeight(property, label, includeChildren: true);
                var propRect = new Rect(position.x, position.y, position.width, propHeight);

                // 先画属性本体（不支持时把字段高亮为红色）
                Color oldColor = GUI.color;
                if (!isSupported) GUI.color = Color.red;
                EditorGUI.PropertyField(propRect, property, label, includeChildren: true);
                GUI.color = oldColor;

                // 不支持时在下方画警告框
                if (!isSupported)
                {
                    float spacing = EditorGUIUtility.standardVerticalSpacing;
                    string msg = $"类型 '{fieldType.Name}' SaveFramework 转换器不支持！请使用自定义转换器。";

                    float helpHeight = EditorStyles.helpBox.CalcHeight(new GUIContent(msg), position.width);
                    var helpRect = new Rect(position.x, propRect.yMax + spacing, position.width, helpHeight);
                    EditorGUI.HelpBox(helpRect, msg, MessageType.Warning);
                }
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var fieldType = fieldInfo.FieldType;
            bool isSupported = IsTypeSupported(fieldType);

            // 属性本体的真实高度
            float propHeight = EditorGUI.GetPropertyHeight(property, label, includeChildren: true);

            if (isSupported)
                return propHeight;

            // 不支持时，把 HelpBox 的高度加上（无法拿到 position.width，估个宽度）
            // 这里用 currentViewWidth 减去一点左右边距作为近似
            float approxWidth = Mathf.Max(100f, EditorGUIUtility.currentViewWidth - 40f);
            string msg = $"类型 '{fieldType.Name}' SaveFramework 转换器不支持！请使用自定义转换器。";
            float helpHeight = EditorStyles.helpBox.CalcHeight(new GUIContent(msg), approxWidth);

            return propHeight + EditorGUIUtility.standardVerticalSpacing + helpHeight;
        }

        private bool IsTypeSupported(System.Type type)
        {
            // 确保注册表在编辑器已初始化（避免第一次进来报告不支持）
            GeneratedConverterRegistry.RegisterAll();

            // 处理可空类型 Nullable<T>
            var underlying = System.Nullable.GetUnderlyingType(type);
            if (underlying != null)
                type = underlying;

            // 基元、字符串、枚举
            if (type.IsPrimitive || type == typeof(string) || type.IsEnum)
                return true;

            // 已注册转换器
            if (ConverterRegistry.HasConverter(type))
                return true;

            // 数组
            if (type.IsArray)
                return IsTypeSupported(type.GetElementType());

            // List<T>
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(System.Collections.Generic.List<>))
                return IsTypeSupported(type.GetGenericArguments()[0]);

            return false;
        }
    }
}
#endif