using System;
using System.IO;
using System.Linq;
using System.Reflection;
using SaveFramework.Runtime.Core.Conversion;
using UnityEditor;
using UnityEngine;

namespace SaveFramework.Editor
{
    public static class ConverterRegistryCodeGen
    {
        private const string GeneratedDir = "Assets/SaveFramework/Runtime/Core/Conversion/Generated";
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
                                         try
                                         {
                                             return a.GetTypes();
                                         }
                                         catch (ReflectionTypeLoadException ex)
                                         {
                                             return ex.Types.Where(t => t != null);
                                         }
                                         catch
                                         {
                                             return Array.Empty<Type>();
                                         }
                                     })
                                     .Where(t => t != null
                                                 && t.GetCustomAttribute<AutoRegisterConverterAttribute>() != null
                                                 && typeof(IValueConverter).IsAssignableFrom(t)
                                                 && !t.IsAbstract && !t.IsInterface)
                                     .Distinct()
                                     .OrderBy(t => t.FullName)
                                     .ToList();

                if (!Directory.Exists(GeneratedDir))
                    Directory.CreateDirectory(GeneratedDir);

                var filePath = Path.Combine(GeneratedDir, GeneratedFile);
                var code = BuildCode(converterTypes);
                File.WriteAllText(filePath, code);
                AssetDatabase.Refresh();

                Debug.Log($"[SaveFramework] Generated {converterTypes.Count} converters to {filePath}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SaveFramework] Codegen failed: {ex}");
            }
        }

        private static string BuildCode(System.Collections.Generic.List<Type> types)
        {
            // 使用 global:: 完整限定名，避免命名空间冲突
            var registrations = string.Join("\n            ", types.Select(t =>
                $"ConverterRegistry.RegisterConverter(new global::{t.FullName}());"));

            return $@"

// 此文件由 SaveFramework.Editor.ConverterRegistryCodeGen 自动生成.
// 请勿手动修改此文件.


using UnityEngine;

namespace {GeneratedNamespace}
{{
    // 在编辑器与运行时两侧都确保已注册
    [UnityEditor.InitializeOnLoad]
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
    }}
}}";
        }
    }
}