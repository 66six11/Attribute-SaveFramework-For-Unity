
// 此文件由 SaveFramework.Editor.ConverterRegistryCodeGen 自动生成.
// 请勿手动修改此文件.

using UnityEngine;

namespace SaveFramework.Runtime.Core.Conversion.Generated
{
#if UNITY_EDITOR
    [UnityEditor.InitializeOnLoad]
#endif
    public static class GeneratedConverterRegistry
    {
        static GeneratedConverterRegistry()
        {
#if UNITY_EDITOR
            // 编辑器脚本重载后立即注册
            RegisterAll();
#endif
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void RuntimeInit()
        {
            RegisterAll();
        }

        public static void RegisterAll()
        {
            // 若已注册任一常见类型，则跳过（避免重复）
            if (SaveFramework.Runtime.Core.Conversion.ConverterRegistry.HasConverter(typeof(UnityEngine.Vector3)))
                return;

            RegisterByName("SaveFramework.Runtime.Core.Conversion.BuiltIn.BoundsConverter, SaveFrameworl.Runtime, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
            RegisterByName("SaveFramework.Runtime.Core.Conversion.BuiltIn.ColorConverter, SaveFrameworl.Runtime, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
            RegisterByName("SaveFramework.Runtime.Core.Conversion.BuiltIn.QuaternionConverter, SaveFrameworl.Runtime, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
            RegisterByName("SaveFramework.Runtime.Core.Conversion.BuiltIn.Vector2Converter, SaveFrameworl.Runtime, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
            RegisterByName("SaveFramework.Runtime.Core.Conversion.BuiltIn.Vector3Converter, SaveFrameworl.Runtime, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
            RegisterByName("SaveFramework.Runtime.Core.Conversion.BuiltIn.Vector4Converter, SaveFrameworl.Runtime, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
        }

        private static void RegisterByName(string assemblyQualifiedName)
        {
            var type = System.Type.GetType(assemblyQualifiedName, throwOnError: false);

            if (type == null)
            {
                // 兜底：遍历已加载程序集（处理部分平台解析不出 AQN 的情况）
                foreach (var asm in System.AppDomain.CurrentDomain.GetAssemblies())
                {
                    var typeName = assemblyQualifiedName.Split(',')[0].Trim();
                    type = asm.GetType(typeName);
                    if (type != null) break;
                }
            }

            if (type == null)
            {
                UnityEngine.Debug.LogWarning($"[SaveFramework] Converter type not found: {assemblyQualifiedName}");
                return;
            }

            try
            {
                var instance = (SaveFramework.Runtime.Core.Conversion.IValueConverter)System.Activator.CreateInstance(type);
                SaveFramework.Runtime.Core.Conversion.ConverterRegistry.RegisterConverter(instance);
            }
            catch (System.Exception ex)
            {
                UnityEngine.Debug.LogError($"[SaveFramework] Failed to instantiate converter {assemblyQualifiedName}: {ex}");
            }
        }
    }
}