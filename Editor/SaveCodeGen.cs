using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using SaveFramework.Runtime.Attributes;
using UnityEditor;
using UnityEngine;
using SaveFramework.Runtime.Core;
using SaveFramework.Runtime.Attributes;

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

        [MenuItem("Tools/Save Framework/生成注册")]
        public static void GenerateRegistration()
        {
            try
            {
                Debug.Log("开始保存框架代码生成...");

                // Ensure the generated folder exists
                if (!Directory.Exists(GeneratedFolderPath))
                {
                    Directory.CreateDirectory(GeneratedFolderPath);
                }

                // Scan for all types with save attributes
                var saveableTypes = ScanForSaveableTypes();

                if (saveableTypes.Count == 0)
                {
                    Debug.LogWarning("未找到具有 [Save] 或 [SaveField] 属性的类型。未生成代码。");
                    return;
                }

                // Generate the registration code
                var generatedCode = GenerateRegistrationCode(saveableTypes);

                // Write to file
                File.WriteAllText(GeneratedFilePath, generatedCode, Encoding.UTF8);

                // Refresh the asset database
                AssetDatabase.Refresh();

                Debug.Log($"保存框架注册生成成功！找到 {saveableTypes.Count} 个可保存类型。文件：{GeneratedFilePath}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"无法生成保存框架注册：{ex.Message}\n{ex.StackTrace}");
            }
        }

        [MenuItem("Tools/Save Framework/清除生成的注册")]
        public static void ClearGeneratedRegistration()
        {
            try
            {
                if (File.Exists(GeneratedFilePath))
                {
                    File.Delete(GeneratedFilePath);
                    AssetDatabase.Refresh();
                    Debug.Log("生成的注册已清除。");
                }
                else
                {
                    Debug.Log("未找到生成的注册文件。");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"无法清除生成的注册： {ex.Message}");
            }
        }

        [MenuItem("Tools/Save Framework/显示生成的文件")]
        public static void ShowGeneratedFile()
        {
            if (File.Exists(GeneratedFilePath))
            {
                EditorUtility.RevealInFinder(GeneratedFilePath);
            }
            else
            {
                Debug.LogWarning("生成的文件不存在。请先生成它。");
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
                    // 某些程序集可能无法正确加载，没关系
                    Debug.LogWarning($"无法从程序集中加载类型 {assembly.FullName}: {ex.Message}");
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"错误扫描程序集：{assembly.FullName}: {ex.Message}");
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
            sb.AppendLine("// 此文件由保存框架代码生成器自动生成");
            sb.AppendLine("// 请勿手动修改此文件，因为更改将被覆盖");
            sb.AppendLine("// 生成时间： " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
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
                    var saveAttr = field.GetCustomAttribute<SaveAttribute>();
                    var key = saveAttr.EffectiveKey(field.Name) ?? field.Name;
                    var aliases = saveAttr.Aliases ?? Array.Empty<string>();

                    var safeFieldId = MakeSafeIdentifier(field.Name);
                    var uniqueSuffix = "_" + i.ToString();
                    var aliasVarName = $"aliases_{safeFieldId}{uniqueSuffix}";
                    var fieldInfoVarName = $"fieldInfo_{safeFieldId}{uniqueSuffix}";

                    sb.AppendLine($"                // 字段: {field.Name}");

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
                        // 公共字段：如果您的 SaveEntry 支持，请使用快速直接 getter/setter lambda
                        sb.AppendLine($"                entries[\"{escapedKey}\"] = new SaveEntry(");
                        sb.AppendLine($"                    \"{escapedKey}\",");
                        sb.AppendLine($"                    {aliasVarName},");
                        sb.AppendLine($"                    typeof({typeName}),");
                        sb.AppendLine($"                    \"{field.Name}\",");
                        sb.AppendLine($"                    typeof({fieldTypeName}),");
                        sb.AppendLine($"                    null, // 直接访问不需要 FieldInfo");
                        sb.AppendLine($"                    obj => (({typeName})obj).{field.Name},");
                        sb.AppendLine($"                    (obj, val) => (({typeName})obj).{field.Name} = ({fieldTypeName})val");
                        sb.AppendLine("                );");
                    }
                    else
                    {
                        // 非公共字段：缓存 FieldInfo
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

            // 自动初始化注册表（属性必须位于方法上）
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

        // 将 System.Type 转换为合法的 C# 源类型名称。
        private static string GetCSharpTypeName(Type type)
        {
            if (type == null) return "object";

            // 可为 null 的<T> -> T？
            var underlyingNullable = Nullable.GetUnderlyingType(type);
            if (underlyingNullable != null)
            {
                var inner = GetCSharpTypeName(underlyingNullable);
                return IsPredefinedValueType(underlyingNullable) ? $"{inner}?" : $"{inner}?";
            }

            // 数组（支持多维）
            if (type.IsArray)
            {
                var elem = GetCSharpTypeName(type.GetElementType());
                int rank = type.GetArrayRank();
                if (rank == 1) return $"{elem}[]";
                return $"{elem}[{new string(',', rank - 1)}]";
            }

            // 泛型
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

            // 通用参数（这里不太可能）
            if (type.IsGenericParameter)
            {
                return type.Name;
            }

            // 预定义别名
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

            // 嵌套类型：对于某些泛型上下文，FullName 可以为 null;回退到 名称
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

        // 从任何字符串创建安全的 C# 标识符（用于生成代码中的本地变量名称）
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

        // C# 字符串文字的转义
        private static string EscapeString(string s)
        {
            if (s == null) return "";
            return s.Replace("\\", "\\\\").Replace("\"", "\\\"");
        }

        // 如果不存在生成的文件，则在脚本重新加载时自动生成
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