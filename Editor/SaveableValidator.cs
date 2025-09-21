#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaveFramework.Runtime.Core.Attributes;
using SaveFramework.Runtime.Core.Conversion;
using UnityEditor;
using UnityEngine;

namespace SaveFramework.Editor
{
    /// <summary>
    /// Validation system for SaveFramework saveable types
    /// </summary>
    public static class SaveableValidator
    {
        [MenuItem("Tools/SaveFramework/Validate All Saveable Types")]
        public static void ValidateAllSaveableTypes()
        {
            var issues = new List<ValidationIssue>();
            
            // Initialize converter registry
            ConverterRegistry.Initialize();
            
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                try
                {
                    var types = assembly.GetTypes()
                        .Where(t => typeof(MonoBehaviour).IsAssignableFrom(t))
                        .Where(HasSaveableFields);
                    
                    foreach (var type in types)
                    {
                        ValidateType(type, issues);
                    }
                }
                catch (ReflectionTypeLoadException)
                {
                    // Skip assemblies that can't be loaded
                }
            }
            
            DisplayValidationResults(issues);
        }

        [MenuItem("Tools/SaveFramework/List Supported Types")]
        public static void ListSupportedTypes()
        {
            ConverterRegistry.Initialize();
            var supportedTypes = ConverterRegistry.GetSupportedTypes();
            
            var message = "SaveFramework Supported Types:\n\n";
            message += "Primitive Types:\n";
            message += "- bool, byte, sbyte, short, ushort, int, uint, long, ulong\n";
            message += "- float, double, decimal, char, string\n";
            message += "- Enums\n\n";
            
            message += "Custom Converters:\n";
            foreach (var type in supportedTypes.OrderBy(t => t.Name))
            {
                message += $"- {type.Name}\n";
            }
            
            message += "\nNote: Arrays and Lists of supported types are also supported.";
            
            EditorUtility.DisplayDialog("SaveFramework Supported Types", message, "OK");
        }

        private static bool HasSaveableFields(Type type)
        {
            var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            return fields.Any(f => f.GetCustomAttribute<SaveAttribute>() != null || 
                                  f.GetCustomAttribute<SaveFieldAttribute>() != null);
        }

        private static void ValidateType(Type type, List<ValidationIssue> issues)
        {
            var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            
            foreach (var field in fields)
            {
                var saveAttr = field.GetCustomAttribute<SaveAttribute>();
                var saveFieldAttr = field.GetCustomAttribute<SaveFieldAttribute>();
                
                if (saveAttr == null && saveFieldAttr == null)
                    continue;
                
                if (!IsTypeSupported(field.FieldType))
                {
                    issues.Add(new ValidationIssue
                    {
                        Severity = ValidationSeverity.Error,
                        Type = type,
                        FieldName = field.Name,
                        FieldType = field.FieldType,
                        Message = $"Field '{field.Name}' in {type.Name} has unsupported type '{field.FieldType.Name}'"
                    });
                }
                else if (IsWarningType(field.FieldType))
                {
                    issues.Add(new ValidationIssue
                    {
                        Severity = ValidationSeverity.Warning,
                        Type = type,
                        FieldName = field.Name,
                        FieldType = field.FieldType,
                        Message = $"Field '{field.Name}' in {type.Name} uses type '{field.FieldType.Name}' which may have serialization limitations"
                    });
                }
            }
        }

        private static bool IsTypeSupported(Type type)
        {
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
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
                return IsTypeSupported(type.GetGenericArguments()[0]);
            
            return false;
        }

        private static bool IsWarningType(Type type)
        {
            // Types that work but may have limitations
            return type == typeof(decimal) || // May lose precision in JSON
                   (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>)); // Not supported by default
        }

        private static void DisplayValidationResults(List<ValidationIssue> issues)
        {
            if (issues.Count == 0)
            {
                EditorUtility.DisplayDialog("Validation Complete", "No issues found! All saveable fields use supported types.", "OK");
                return;
            }
            
            var errors = issues.Where(i => i.Severity == ValidationSeverity.Error).ToList();
            var warnings = issues.Where(i => i.Severity == ValidationSeverity.Warning).ToList();
            
            var message = "";
            
            if (errors.Count > 0)
            {
                message += $"ERRORS ({errors.Count}):\n";
                foreach (var error in errors.Take(10)) // Limit to first 10
                {
                    message += $"• {error.Message}\n";
                }
                if (errors.Count > 10)
                    message += $"• ... and {errors.Count - 10} more errors\n";
                message += "\n";
            }
            
            if (warnings.Count > 0)
            {
                message += $"WARNINGS ({warnings.Count}):\n";
                foreach (var warning in warnings.Take(5)) // Limit to first 5
                {
                    message += $"• {warning.Message}\n";
                }
                if (warnings.Count > 5)
                    message += $"• ... and {warnings.Count - 5} more warnings\n";
            }
            
            var title = errors.Count > 0 ? "Validation Failed" : "Validation Warnings";
            EditorUtility.DisplayDialog(title, message, "OK");
            
            // Log detailed issues to console
            foreach (var issue in issues)
            {
                if (issue.Severity == ValidationSeverity.Error)
                    Debug.LogError($"[SaveFramework] {issue.Message}");
                else
                    Debug.LogWarning($"[SaveFramework] {issue.Message}");
            }
        }
        
        /// <summary>
        /// Build processor to validate saveable types before build
        /// </summary>
        [UnityEditor.Build.IPreprocessBuildWithReport]
        public class SaveFrameworkBuildProcessor : UnityEditor.Build.IPreprocessBuildWithReport
        {
            public int callbackOrder => 0;

            public void OnPreprocessBuild(UnityEditor.Build.Reporting.BuildReport report)
            {
                var issues = new List<ValidationIssue>();
                ConverterRegistry.Initialize();
                
                var assemblies = AppDomain.CurrentDomain.GetAssemblies();
                foreach (var assembly in assemblies)
                {
                    try
                    {
                        var types = assembly.GetTypes()
                            .Where(t => typeof(MonoBehaviour).IsAssignableFrom(t))
                            .Where(HasSaveableFields);
                        
                        foreach (var type in types)
                        {
                            ValidateType(type, issues);
                        }
                    }
                    catch (ReflectionTypeLoadException)
                    {
                        // Skip assemblies that can't be loaded
                    }
                }
                
                var errors = issues.Where(i => i.Severity == ValidationSeverity.Error).ToList();
                if (errors.Count > 0)
                {
                    var errorMessage = $"Build failed: SaveFramework found {errors.Count} unsupported field types:\n";
                    foreach (var error in errors.Take(5))
                    {
                        errorMessage += $"• {error.Message}\n";
                    }
                    if (errors.Count > 5)
                        errorMessage += $"• ... and {errors.Count - 5} more errors";
                    
                    throw new UnityEditor.Build.BuildFailedException(errorMessage);
                }
            }
        }
    }

    public class ValidationIssue
    {
        public ValidationSeverity Severity;
        public Type Type;
        public string FieldName;
        public Type FieldType;
        public string Message;
    }

    public enum ValidationSeverity
    {
        Warning,
        Error
    }
}
#endif