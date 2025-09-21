using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;
using SaveFramework.Runtime.Core;

namespace SaveFramework.Editor
{
    /// <summary>
    /// Editor tool for generating save framework registration code
    /// </summary>
    public static class SaveCodeGen
    {
        private const string GeneratedFolderPath = "Assets/SaveFramework/Generated";
        private const string GeneratedFileName = "SaveFramework.Generated.cs";
        private const string GeneratedFilePath = GeneratedFolderPath + "/" + GeneratedFileName;

        [MenuItem("Tools/Save Framework/Generate Registration")]
        public static void GenerateRegistration()
        {
            try
            {
                Debug.Log("Starting save framework code generation...");

                // Ensure the generated folder exists
                if (!Directory.Exists(GeneratedFolderPath))
                {
                    Directory.CreateDirectory(GeneratedFolderPath);
                }

                // Scan for all types with save attributes
                var saveableTypes = ScanForSaveableTypes();

                if (saveableTypes.Count == 0)
                {
                    Debug.LogWarning("No types with [Save] or [SaveField] attributes found. No code generated.");
                    return;
                }

                // Generate the registration code
                var generatedCode = GenerateRegistrationCode(saveableTypes);

                // Write to file
                File.WriteAllText(GeneratedFilePath, generatedCode, Encoding.UTF8);

                // Refresh the asset database
                AssetDatabase.Refresh();

                Debug.Log($"Save framework registration generated successfully! Found {saveableTypes.Count} saveable types. File: {GeneratedFilePath}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to generate save framework registration: {ex.Message}\n{ex.StackTrace}");
            }
        }

        [MenuItem("Tools/Save Framework/Clear Generated Registration")]
        public static void ClearGeneratedRegistration()
        {
            try
            {
                if (File.Exists(GeneratedFilePath))
                {
                    File.Delete(GeneratedFilePath);
                    AssetDatabase.Refresh();
                    Debug.Log("Generated registration cleared.");
                }
                else
                {
                    Debug.Log("No generated registration file found.");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to clear generated registration: {ex.Message}");
            }
        }

        [MenuItem("Tools/Save Framework/Show Generated File")]
        public static void ShowGeneratedFile()
        {
            if (File.Exists(GeneratedFilePath))
            {
                EditorUtility.RevealInFinder(GeneratedFilePath);
            }
            else
            {
                Debug.LogWarning("Generated file does not exist. Generate it first.");
            }
        }

        private static Dictionary<Type, List<FieldInfo>> ScanForSaveableTypes()
        {
            var saveableTypes = new Dictionary<Type, List<FieldInfo>>();

            // Get all assemblies
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (var assembly in assemblies)
            {
                try
                {
                    // Skip dynamic or reflection-only assemblies
                    if (assembly.IsDynamic) continue;

                    foreach (var type in assembly.GetTypes())
                    {
                        // Skip non-MonoBehaviour types
                        if (!typeof(MonoBehaviour).IsAssignableFrom(type))
                            continue;

                        var saveableFields = GetSaveableFields(type);
                        if (saveableFields.Count > 0)
                        {
                            saveableTypes[type] = saveableFields;
                        }
                    }
                }
                catch (ReflectionTypeLoadException ex)
                {
                    // Some assemblies might not load properly, that's okay
                    Debug.LogWarning($"Could not load types from assembly {assembly.FullName}: {ex.Message}");
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"Error scanning assembly {assembly.FullName}: {ex.Message}");
                }
            }

            return saveableTypes;
        }

        private static List<FieldInfo> GetSaveableFields(Type type)
        {
            var saveableFields = new List<FieldInfo>();
            var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            foreach (var field in fields)
            {
                var saveAttr = field.GetCustomAttribute<SaveAttribute>();
                if (saveAttr == null)
                {
                    saveAttr = field.GetCustomAttribute<SaveFieldAttribute>();
                }

                if (saveAttr != null && Converters.IsSupported(field.FieldType))
                {
                    saveableFields.Add(field);
                }
            }

            return saveableFields;
        }

        private static string GenerateRegistrationCode(Dictionary<Type, List<FieldInfo>> saveableTypes)
        {
            var sb = new StringBuilder();

            // File header
            sb.AppendLine("// This file is auto-generated by Save Framework Code Generator");
            sb.AppendLine("// Do not modify this file manually as changes will be overwritten");
            sb.AppendLine("// Generation time: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            sb.AppendLine();
            sb.AppendLine("using System;");
            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine("using SaveFramework.Runtime.Core;");
            sb.AppendLine("using UnityEngine;");
            sb.AppendLine();
            sb.AppendLine("namespace SaveFramework.Generated");
            sb.AppendLine("{");

            // Generate the registry class
            sb.AppendLine("    public class SaveFrameworkRegistry : GeneratedSaveRegistry");
            sb.AppendLine("    {");
            sb.AppendLine("        private static readonly Type[] HandledTypes = new Type[]");
            sb.AppendLine("        {");

            foreach (var type in saveableTypes.Keys)
            {
                sb.AppendLine($"            typeof({GetCSharpTypeName(type)}),");
            }

            sb.AppendLine("        };");
            sb.AppendLine();

            sb.AppendLine("        public override bool HandlesType(Type componentType)");
            sb.AppendLine("        {");
            sb.AppendLine("            foreach (var handledType in HandledTypes)");
            sb.AppendLine("            {");
            sb.AppendLine("                if (handledType == componentType)");
            sb.AppendLine("                    return true;");
            sb.AppendLine("            }");
            sb.AppendLine("            return false;");
            sb.AppendLine("        }");
            sb.AppendLine();

            sb.AppendLine("        public override Type[] GetHandledTypes()");
            sb.AppendLine("        {");
            sb.AppendLine("            return HandledTypes;");
            sb.AppendLine("        }");
            sb.AppendLine();

            sb.AppendLine("        public override void RegisterSaveEntries(Type componentType, Dictionary<string, SaveEntry> entries)");
            sb.AppendLine("        {");

            foreach (var kvp in saveableTypes)
            {
                var type = kvp.Key;
                var fields = kvp.Value;

                sb.AppendLine($"            if (componentType == typeof({GetCSharpTypeName(type)}))");
                sb.AppendLine("            {");

                for (int i = 0; i < fields.Count; i++)
                {
                    var field = fields[i];
                    var saveAttr = field.GetCustomAttribute<SaveAttribute>() ?? field.GetCustomAttribute<SaveFieldAttribute>();
                    var key = saveAttr.EffectiveKey(field.Name) ?? field.Name;
                    var aliases = saveAttr.Aliases ?? Array.Empty<string>();

                    var safeFieldId = MakeSafeIdentifier(field.Name);
                    var uniqueSuffix = "_" + i.ToString();
                    var aliasVarName = $"aliases_{safeFieldId}{uniqueSuffix}";
                    var fieldInfoVarName = $"fieldInfo_{safeFieldId}{uniqueSuffix}";

                    sb.AppendLine($"                // Field: {field.Name}");

                    // Create aliases array
                    if (aliases.Length > 0)
                    {
                        sb.Append($"                var {aliasVarName} = new string[] {{ ");
                        sb.Append(string.Join(", ", aliases.Select(a => $"\"{EscapeString(a)}\"")));
                        sb.AppendLine(" };");
                    }
                    else
                    {
                        sb.AppendLine($"                var {aliasVarName} = new string[0];");
                    }

                    var typeName = GetCSharpTypeName(type);
                    var fieldTypeName = GetCSharpTypeName(field.FieldType);
                    var escapedKey = EscapeString(key);

                    if (field.IsPublic)
                    {
                        // Public fields: use fast direct getter/setter lambdas if your SaveEntry supports them
                        sb.AppendLine($"                entries[\"{escapedKey}\"] = new SaveEntry(");
                        sb.AppendLine($"                    \"{escapedKey}\",");
                        sb.AppendLine($"                    {aliasVarName},");
                        sb.AppendLine($"                    typeof({typeName}),");
                        sb.AppendLine($"                    \"{field.Name}\",");
                        sb.AppendLine($"                    typeof({fieldTypeName}),");
                        sb.AppendLine($"                    null, // FieldInfo not needed for direct access");
                        sb.AppendLine($"                    obj => (({typeName})obj).{field.Name},");
                        sb.AppendLine($"                    (obj, val) => (({typeName})obj).{field.Name} = ({fieldTypeName})val");
                        sb.AppendLine("                );");
                    }
                    else
                    {
                        // Non-public fields: cache FieldInfo
                        sb.AppendLine($"                var {fieldInfoVarName} = typeof({typeName}).GetField(\"{field.Name}\", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);");
                        sb.AppendLine($"                entries[\"{escapedKey}\"] = new SaveEntry(");
                        sb.AppendLine($"                    \"{escapedKey}\",");
                        sb.AppendLine($"                    {aliasVarName},");
                        sb.AppendLine($"                    typeof({typeName}),");
                        sb.AppendLine($"                    \"{field.Name}\",");
                        sb.AppendLine($"                    typeof({fieldTypeName}),");
                        sb.AppendLine($"                    {fieldInfoVarName}");
                        sb.AppendLine("                );");
                    }

                    sb.AppendLine();
                }

                sb.AppendLine("                return;");
                sb.AppendLine("            }");
                sb.AppendLine();
            }

            sb.AppendLine("        }");
            sb.AppendLine("    }");
            sb.AppendLine();

            // Auto-initialize the registry (attribute must be on the method)
            sb.AppendLine("    public static class SaveFrameworkAutoInit");
            sb.AppendLine("    {");
            sb.AppendLine("        [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.BeforeSceneLoad)]");
            sb.AppendLine("        private static void Initialize()");
            sb.AppendLine("        {");
            sb.AppendLine("            SaveRegistryManager.RegisterRegistry(new SaveFrameworkRegistry());");
            sb.AppendLine("        }");
            sb.AppendLine("    }");

            sb.AppendLine("}");

            return sb.ToString();
        }

        // Convert a System.Type to a legal C# source type name.
        private static string GetCSharpTypeName(Type type)
        {
            if (type == null) return "object";

            // Nullable<T> -> T?
            var underlyingNullable = Nullable.GetUnderlyingType(type);
            if (underlyingNullable != null)
            {
                var inner = GetCSharpTypeName(underlyingNullable);
                return IsPredefinedValueType(underlyingNullable) ? $"{inner}?" : $"{inner}?";
            }

            // Array (support multi-dimensional)
            if (type.IsArray)
            {
                var elem = GetCSharpTypeName(type.GetElementType());
                int rank = type.GetArrayRank();
                if (rank == 1) return $"{elem}[]";
                return $"{elem}[{new string(',', rank - 1)}]";
            }

            // Generic
            if (type.IsGenericType)
            {
                var genericDef = type.GetGenericTypeDefinition();
                var baseName = (genericDef.FullName ?? genericDef.Name);
                int tickIndex = baseName.IndexOf('`');
                if (tickIndex >= 0) baseName = baseName.Substring(0, tickIndex);
                baseName = baseName.Replace('+', '.'); // nested type separator

                var args = type.GetGenericArguments().Select(GetCSharpTypeName);
                return $"{baseName}<{string.Join(", ", args)}>";
            }

            // Generic parameter (unlikely here)
            if (type.IsGenericParameter)
            {
                return type.Name;
            }

            // Predefined aliases
            if (type == typeof(void)) return "void";
            if (type == typeof(bool)) return "bool";
            if (type == typeof(byte)) return "byte";
            if (type == typeof(sbyte)) return "sbyte";
            if (type == typeof(short)) return "short";
            if (type == typeof(ushort)) return "ushort";
            if (type == typeof(int)) return "int";
            if (type == typeof(uint)) return "uint";
            if (type == typeof(long)) return "long";
            if (type == typeof(ulong)) return "ulong";
            if (type == typeof(float)) return "float";
            if (type == typeof(double)) return "double";
            if (type == typeof(decimal)) return "decimal";
            if (type == typeof(char)) return "char";
            if (type == typeof(string)) return "string";
            if (type == typeof(object)) return "object";

            // Nested types: FullName can be null for some generic contexts; fall back to Name
            var full = type.FullName ?? type.Name;
            return full.Replace('+', '.');
        }

        private static bool IsPredefinedValueType(Type t)
        {
            return t == typeof(bool) || t == typeof(byte) || t == typeof(sbyte) ||
                   t == typeof(short) || t == typeof(ushort) || t == typeof(int) ||
                   t == typeof(uint) || t == typeof(long) || t == typeof(ulong) ||
                   t == typeof(float) || t == typeof(double) || t == typeof(decimal) ||
                   t == typeof(char);
        }

        // Make a safe C# identifier from any string (for local var names in generated code)
        private static string MakeSafeIdentifier(string name)
        {
            if (string.IsNullOrEmpty(name)) return "_";
            var sb = new StringBuilder(name.Length);
            // First char: must be letter or '_'
            char first = name[0];
            if (char.IsLetter(first) || first == '_') sb.Append(first);
            else sb.Append('_');

            for (int i = 1; i < name.Length; i++)
            {
                char c = name[i];
                if (char.IsLetterOrDigit(c) || c == '_') sb.Append(c);
                else sb.Append('_');
            }

            return sb.ToString();
        }

        // Escape for C# string literal
        private static string EscapeString(string s)
        {
            if (s == null) return "";
            return s.Replace("\\", "\\\\").Replace("\"", "\\\"");
        }

        // Auto-generate on scripts reload if no generated file exists
        [InitializeOnLoadMethod]
        private static void AutoGenerateOnStartup()
        {
            if (!File.Exists(GeneratedFilePath))
            {
                EditorApplication.delayCall += () =>
                {
                    Debug.Log("Auto-generating save framework registration on startup...");
                    GenerateRegistration();
                };
            }
        }
    }
}