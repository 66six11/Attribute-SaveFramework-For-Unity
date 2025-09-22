using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using SaveFramework.Runtime.Core.Conversion;
using UnityEditor;
using UnityEngine;

namespace SaveFramework.Editor
{
    public static class ConverterRegistryCodeGen
    {
        // 仅作兜底（找不到 asmdef 时使用）
        private const string DefaultGeneratedDir = "Assets/SaveFramework/Runtime/Core/Conversion/Generated";
        private const string GeneratedFile = "GeneratedConverterRegistry.cs";
        private const string GeneratedNamespace = "SaveFramework.Runtime.Core.Conversion.Generated";

        [MenuItem("Tools/Save Framework/预生成转换器注册表 （Codegen）")]
        public static void Generate()
        {
            try
            {
                var assemblies = AppDomain.CurrentDomain.GetAssemblies();
                var converterTypes = assemblies
                    .SelectMany(a =>
                    {
                        try { return a.GetTypes(); }
                        catch (ReflectionTypeLoadException ex) { return ex.Types.Where(t => t != null); }
                        catch { return Array.Empty<Type>(); }
                    })
                    .Where(t => t != null
                                && t.GetCustomAttribute<AutoRegisterConverterAttribute>() != null
                                && typeof(IValueConverter).IsAssignableFrom(t)
                                && !t.IsAbstract && !t.IsInterface)
                    .Distinct()
                    .OrderBy(t => t.FullName)
                    .ToList();

                // 解析“相对 Runtime asmdef”的目标生成目录
                var assetDir = ResolveGeneratedAssetDir() ?? DefaultGeneratedDir;

                // 确保目录存在
                EnsureAssetDirectory(assetDir, out var absDir);

                var assetFilePath = CombineAssetPath(assetDir, GeneratedFile);
                var absFilePath = PathCombineAbsolute(absDir, GeneratedFile);

                // 生成代码并写入
                var code = BuildCode(converterTypes);
                File.WriteAllText(absFilePath, code);

                // 刷新导入
                AssetDatabase.ImportAsset(assetFilePath);
                AssetDatabase.Refresh();

                Debug.Log($"[SaveFramework] Generated {converterTypes.Count} converters to {assetFilePath}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SaveFramework] Codegen failed: {ex}");
            }
        }

        // 关键：根据 Runtime asmdef 所在目录，拼出 Core/Conversion/Generated
        private static string ResolveGeneratedAssetDir()
        {
            try
            {
                // 取 Runtime 程序集名（IValueConverter 所在程序集）
                var runtimeAsmName = typeof(IValueConverter).Assembly.GetName().Name;

                // 找到对应的 asmdef 资产
                var guids = AssetDatabase.FindAssets("t:asmdef");
                foreach (var guid in guids)
                {
                    var asmdefPath = AssetDatabase.GUIDToAssetPath(guid);
                    var text = AssetDatabase.LoadAssetAtPath<TextAsset>(asmdefPath);
                    if (text == null) continue;

                    // 粗略解析 name 字段（无需完整 JSON 反序列化）
                    if (!ContainsAsmdefName(text.text, runtimeAsmName)) continue;

                    // asmdef 所在目录即为 Runtime 根目录
                    var runtimeDir = GetDirectoryNameAsset(asmdefPath); // e.g. ".../SaveFramework/Runtime"
                    if (string.IsNullOrEmpty(runtimeDir)) continue;

                    // 拼接固定的子路径
                    var target = CombineAssetPath(runtimeDir, "Core/Conversion/Generated");
                    return NormalizeAssetPath(target);
                }

                Debug.LogWarning($"[SaveFramework] 未找到名称为 {runtimeAsmName} 的 asmdef，使用默认路径: {DefaultGeneratedDir}");
                return null;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[SaveFramework] 解析 Runtime asmdef 失败，将使用默认路径。原因: {ex.Message}");
                return null;
            }
        }

        // 在 asmdef JSON 中判断 name 是否匹配（容错空白）
        private static bool ContainsAsmdefName(string json, string name)
        {
            if (string.IsNullOrEmpty(json) || string.IsNullOrEmpty(name)) return false;
            // 简单匹配："name": "Assembly.Name"
            // 允许空白与不同引号间距
            var needle = $"\"name\"";
            var idx = json.IndexOf(needle, StringComparison.Ordinal);
            while (idx >= 0)
            {
                // 从 idx 往后找第一个引号包围的值
                var after = json.AsSpan(idx + needle.Length);
                // 跳过空白和冒号
                int i = 0;
                while (i < after.Length && (char.IsWhiteSpace(after[i]) || after[i] == ':')) i++;
                if (i < after.Length && after[i] == '\"')
                {
                    i++;
                    int start = i;
                    while (i < after.Length && after[i] != '\"') i++;
                    if (i <= after.Length)
                    {
                        var val = after.Slice(start, i - start).ToString();
                        if (string.Equals(val, name, StringComparison.Ordinal)) return true;
                    }
                }
                idx = json.IndexOf(needle, idx + needle.Length, StringComparison.Ordinal);
            }
            return false;
        }

        private static string BuildCode(List<Type> types)
        {
            // 按名称注册，避免对外部程序集产生编译期依赖
            var names = types
                .Select(t => t.AssemblyQualifiedName?.Replace("\"", "\"\""))
                .Where(s => !string.IsNullOrEmpty(s))
                .ToList();

            var registrations = string.Join("\n            ", names.Select(n => $"RegisterByName(\"{n}\");"));

            return $@"
// 此文件由 SaveFramework.Editor.ConverterRegistryCodeGen 自动生成.
// 请勿手动修改此文件.

using UnityEngine;

namespace {GeneratedNamespace}
{{
#if UNITY_EDITOR
    [UnityEditor.InitializeOnLoad]
#endif
    public static class GeneratedConverterRegistry
    {{
        static GeneratedConverterRegistry()
        {{
#if UNITY_EDITOR
            // 编辑器脚本重载后立即注册
            RegisterAll();
#endif
        }}

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void RuntimeInit()
        {{
            RegisterAll();
        }}

        public static void RegisterAll()
        {{
            // 若已注册任一常见类型，则跳过（避免重复）
            if (SaveFramework.Runtime.Core.Conversion.ConverterRegistry.HasConverter(typeof(UnityEngine.Vector3)))
                return;

            {registrations}
        }}

        private static void RegisterByName(string assemblyQualifiedName)
        {{
            var type = System.Type.GetType(assemblyQualifiedName, throwOnError: false);

            if (type == null)
            {{
                // 兜底：遍历已加载程序集（处理部分平台解析不出 AQN 的情况）
                foreach (var asm in System.AppDomain.CurrentDomain.GetAssemblies())
                {{
                    var typeName = assemblyQualifiedName.Split(',')[0].Trim();
                    type = asm.GetType(typeName);
                    if (type != null) break;
                }}
            }}

            if (type == null)
            {{
                UnityEngine.Debug.LogWarning($""[SaveFramework] Converter type not found: {{assemblyQualifiedName}}"");
                return;
            }}

            try
            {{
                var instance = (SaveFramework.Runtime.Core.Conversion.IValueConverter)System.Activator.CreateInstance(type);
                SaveFramework.Runtime.Core.Conversion.ConverterRegistry.RegisterConverter(instance);
            }}
            catch (System.Exception ex)
            {{
                UnityEngine.Debug.LogError($""[SaveFramework] Failed to instantiate converter {{assemblyQualifiedName}}: {{ex}}"");
            }}
        }}
    }}
}}";
        }

        // ————— 路径/目录工具 —————

         private static void EnsureAssetDirectory(string assetDir, out string absDir)
        {
            assetDir = NormalizeAssetPath(assetDir);
            absDir = ToAbsolutePath(assetDir);
            if (absDir == null)
                throw new InvalidOperationException($"无法解析生成目录的绝对路径: {assetDir}");

            // 注意：Assets 下不要用 Directory.CreateDirectory；只用 AssetDatabase.CreateFolder
            if (assetDir.StartsWith("Assets/") || assetDir.Equals("Assets", StringComparison.Ordinal))
            {
                EnsureAssetFoldersAssetDatabase(assetDir);
                return;
            }

            // Packages（嵌入式包）用文件系统创建，并刷新
            if (assetDir.StartsWith("Packages/") || assetDir.Equals("Packages", StringComparison.Ordinal))
            {
                if (!Directory.Exists(absDir))
                    Directory.CreateDirectory(absDir);
                AssetDatabase.Refresh(); // 让 Unity 识别新目录
                return;
            }

            throw new InvalidOperationException($"不支持的输出根路径: {assetDir}（仅支持 Assets/ 或嵌入式 Packages/）");
        }

        // 仅适用于 Assets/ 下逐级创建，避免出现 “Generated 1”
        private static void EnsureAssetFoldersAssetDatabase(string assetPath)
        {
            assetPath = NormalizeAssetPath(assetPath);
            if (!assetPath.StartsWith("Assets/") && !assetPath.Equals("Assets", StringComparison.Ordinal))
                return;

            var parts = assetPath.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            var curr = "Assets";

            // 如果目标就是 Assets 根，直接返回
            if (parts.Length == 1 && parts[0] == "Assets")
                return;

            for (int i = 1; i < parts.Length; i++)
            {
                var next = $"{curr}/{parts[i]}";
                if (AssetDatabase.IsValidFolder(next))
                {
                    curr = next;
                    continue;
                }

                // 若文件系统已有此目录但 Unity 尚未识别（缺 .meta），先导入/刷新再判断
                var absNext = ToAbsolutePath(next);
                if (!string.IsNullOrEmpty(absNext) && Directory.Exists(absNext))
                {
                    AssetDatabase.ImportAsset(next, ImportAssetOptions.ForceSynchronousImport);
                    if (AssetDatabase.IsValidFolder(next))
                    {
                        curr = next;
                        continue;
                    }
                }

                // 真正创建（这一步不会再产生 “ 1”，因为此前确保不存在已识别文件夹）
                AssetDatabase.CreateFolder(curr, parts[i]);
                curr = next;
            }
        }

        private static string GetDirectoryNameAsset(string assetPath)
        {
            assetPath = NormalizeAssetPath(assetPath);
            var idx = assetPath.LastIndexOf('/');
            return idx >= 0 ? assetPath.Substring(0, idx) : string.Empty;
        }

        private static string CombineAssetPath(string baseDir, string sub)
        {
            baseDir = NormalizeAssetPath(baseDir);
            sub = NormalizeAssetPath(sub);
            if (string.IsNullOrEmpty(baseDir)) return sub;
            if (string.IsNullOrEmpty(sub)) return baseDir;
            return $"{baseDir}/{sub}";
        }

        private static string NormalizeAssetPath(string p)
        {
            return p?.Replace('\\', '/').Trim().TrimEnd('/');
        }

        private static string ToAbsolutePath(string assetPath)
        {
            if (string.IsNullOrEmpty(assetPath)) return null;
            assetPath = NormalizeAssetPath(assetPath);

            var dataPath = Application.dataPath.Replace('\\', '/');
            var projectRoot = dataPath.Substring(0, dataPath.Length - "/Assets".Length);

            if (assetPath.StartsWith("Assets"))
                return Path.GetFullPath(Path.Combine(projectRoot, assetPath));

            if (assetPath.StartsWith("Packages"))
                return Path.GetFullPath(Path.Combine(projectRoot, assetPath));

            return null;
        }

        private static string PathCombineAbsolute(string dirAbs, string fileName)
        {
            return Path.GetFullPath(Path.Combine(dirAbs, fileName));
        }
    }
}